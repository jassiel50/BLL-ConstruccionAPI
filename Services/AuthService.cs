using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BLL_ConstruccionAPI.DTOs.Auth;
using BLL_ConstruccionAPI.Helpers;
using BLL_ConstruccionAPI.Models.Auth;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using OtpNet;

namespace BLL_ConstruccionAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;
    private readonly MfaTicketHelper _mfaTicketHelper;
    private readonly RateLimitHelper _rateLimitHelper;

    public AuthService(
        IUsuarioRepository usuarioRepo, 
        IConfiguration config,
        IEmailService emailService)
    {
        _usuarioRepo = usuarioRepo;
        _config = config;
        _emailService = emailService;
        _mfaTicketHelper = new MfaTicketHelper(config);
        _rateLimitHelper = new RateLimitHelper();
    }

    // ─── REGISTER ────────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto dto, string ipOrigen)
    {
        if (await _usuarioRepo.ExisteNombreUsuarioAsync(dto.NombreUsuario))
            return (false, "El nombre de usuario ya existe.");

        if (await _usuarioRepo.ExisteEmailAsync(dto.Email))
            return (false, "El email ya está registrado.");

        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario,
            Nombre = dto.Nombre,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            RolId = dto.RolId,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _usuarioRepo.CreateAsync(usuario);
        return (true, "Usuario registrado correctamente.");
    }

    // ─── LOGIN ────────────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginRequestDto dto, string ipOrigen)
    {
        var usuario = await _usuarioRepo.GetByNombreUsuarioAsync(dto.NombreUsuario);

        if (usuario is null || !VerifyPassword(dto.Password, usuario.PasswordHash))
        {
            if (usuario is not null)
                await _usuarioRepo.RegistrarLogAsync(new LogAcceso
                {
                    UsuarioId = usuario.Id,
                    Fecha = DateTime.UtcNow,
                    Exitoso = false,
                    IpOrigen = ipOrigen,
                    Descripcion = "Credenciales incorrectas"
                });

            return (false, "Usuario o contraseña incorrectos.", null);
        }

        if (!usuario.Activo)
            return (false, "Usuario inactivo. Contacta al administrador.", null);

        // Verificar configuración MFA
        var mfaConfig = await _usuarioRepo.GetMfaConfigAsync(usuario.Id);
        var config2FALegacy = await _usuarioRepo.Get2FAAsync(usuario.Id);

        // Migración automática: usuarios con 2FA legacy
        if (config2FALegacy?.Habilitado == true && mfaConfig is null)
        {
            mfaConfig = new UsuarioMfaConfig
            {
                UsuarioId = usuario.Id,
                MfaHabilitado = true,
                MfaMetodoPreferido = "app",
                MfaAppHabilitado = true,
                MfaEmailHabilitado = false,
                MfaUltimaActualizacion = DateTime.UtcNow
            };
            await _usuarioRepo.CreateMfaConfigAsync(mfaConfig);
        }

        // Generar MFA ticket para continuar el flujo
        var mfaTicket = new MfaTicket
        {
            UsuarioId = usuario.Id,
            Email = usuario.Email,
            NombreUsuario = usuario.NombreUsuario
        };
        var ticketToken = _mfaTicketHelper.GenerarMfaTicket(mfaTicket);

        // No tiene MFA configurado → debe elegir método
        if (mfaConfig is null || !mfaConfig.MfaHabilitado)
        {
            return (true, "Debes configurar MFA para continuar.", new LoginResponseDto
            {
                RequiereMfa = true,
                MfaConfigRequerida = true,
                MfaTicket = ticketToken,
                MaskedEmail = OtpHelper.MaskEmail(usuario.Email),
                UsuarioId = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                // Backward compatibility
                Requiere2FA = true,
                Configurado2FA = false
            });
        }

        // Ya tiene MFA configurado → pedir verificación
        return (true, "Se requiere verificación MFA.", new LoginResponseDto
        {
            RequiereMfa = true,
            MfaConfigRequerida = false,
            MfaMetodoSugerido = mfaConfig.MfaMetodoPreferido,
            MfaTicket = ticketToken,
            MaskedEmail = OtpHelper.MaskEmail(usuario.Email),
            UsuarioId = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            // Backward compatibility
            Requiere2FA = true,
            Configurado2FA = true
        });
    }

    // ─── VERIFY 2FA ──────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, LoginResponseDto? Data)> Verify2FAAsync(Verify2FARequestDto dto, string ipOrigen)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(dto.UsuarioId);
        if (usuario is null)
            return (false, "Usuario no encontrado.", null);

        var config2FA = await _usuarioRepo.Get2FAAsync(dto.UsuarioId);
        if (config2FA is null || !config2FA.Habilitado)
            return (false, "El usuario no tiene 2FA activo.", null);

        if (!ValidarCodigo2FA(config2FA.SecretKey, dto.Codigo))
        {
            await _usuarioRepo.RegistrarLogAsync(new LogAcceso
            {
                UsuarioId = usuario.Id,
                Fecha = DateTime.UtcNow,
                Exitoso = false,
                IpOrigen = ipOrigen,
                Descripcion = "Código 2FA incorrecto"
            });

            return (false, "Código 2FA incorrecto.", null);
        }

        var response = await GenerarTokensAsync(usuario, ipOrigen);
        return (true, "Login exitoso.", response);
    }


    

    // ─── REFRESH TOKEN ────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var tokenSesion = await _usuarioRepo.GetTokenAsync(dto.RefreshToken);

        if (tokenSesion is null)
            return (false, "Refresh token inválido o revocado.", null);

        if (tokenSesion.FechaExpira < DateTime.UtcNow)
        {
            await _usuarioRepo.RevocarTokenAsync(dto.RefreshToken);
            return (false, "Refresh token expirado.", null);
        }

        var usuario = await _usuarioRepo.GetByIdAsync(tokenSesion.UsuarioId);
        if (usuario is null || !usuario.Activo)
            return (false, "Usuario no encontrado o inactivo.", null);

        await _usuarioRepo.RevocarTokenAsync(dto.RefreshToken);
        var response = await GenerarTokensAsync(usuario, tokenSesion.IpOrigen ?? "");
        return (true, "Token renovado.", response);
    }

    // ─── LOGOUT ───────────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message)> LogoutAsync(RefreshTokenRequestDto dto)
    {
        await _usuarioRepo.RevocarTokenAsync(dto.RefreshToken);
        return (true, "Sesión cerrada correctamente.");
    }

    // ─── ENABLE 2FA ───────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, Enable2FAResponseDto? Data)> Enable2FAAsync(int usuarioId)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId);
        if (usuario is null)
            return (false, "Usuario no encontrado.", null);

        var secretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

        var config2FA = await _usuarioRepo.Get2FAAsync(usuarioId);
        if (config2FA is null)
        {
            await _usuarioRepo.Create2FAAsync(new Usuario2FA
            {
                UsuarioId = usuarioId,
                SecretKey = secretKey,
                Habilitado = false
            });
        }
        else
        {
            config2FA.SecretKey = secretKey;
            config2FA.Habilitado = false;
            await _usuarioRepo.Update2FAAsync(config2FA);
        }

        var qrUrl = $"otpauth://totp/BLL-Construccion:{usuario.NombreUsuario}?secret={secretKey}&issuer=BLL-Construccion";

        return (true, "Escanea el QR con Google Authenticator.", new Enable2FAResponseDto
        {
            SecretKey = secretKey,
            QrCodeUrl = qrUrl
        });
    }

    // ─── CONFIRM 2FA ──────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message)> Confirm2FAAsync(Verify2FARequestDto dto)
    {
        var config2FA = await _usuarioRepo.Get2FAAsync(dto.UsuarioId);
        if (config2FA is null)
            return (false, "Primero activa el 2FA.");

        if (!ValidarCodigo2FA(config2FA.SecretKey, dto.Codigo))
            return (false, "Código incorrecto. Intenta de nuevo.");

        config2FA.Habilitado = true;
        config2FA.FechaActivado = DateTime.UtcNow;
        await _usuarioRepo.Update2FAAsync(config2FA);

        return (true, "2FA activado correctamente.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NUEVOS MÉTODOS MFA FLEXIBLE
    // ═══════════════════════════════════════════════════════════════════════════

    // ─── SELECT MFA METHOD ────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, Enable2FAResponseDto? Data)> SelectMfaMethodAsync(
        SelectMfaMethodRequestDto dto, string ipOrigen)
    {
        var (valid, ticket) = _mfaTicketHelper.ValidarMfaTicket(dto.MfaTicket);
        if (!valid || ticket is null)
            return (false, "Ticket MFA inválido o expirado.", null);

        var usuario = await _usuarioRepo.GetByIdAsync(ticket.UsuarioId);
        if (usuario is null)
            return (false, "Usuario no encontrado.", null);

        var metodo = dto.Metodo.ToLower();
        if (metodo != "app" && metodo != "email")
            return (false, "Método no válido. Usa 'app' o 'email'.", null);

        // Método APP → generar QR
        if (metodo == "app")
        {
            var secretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

            // Guardar en Usuario2FA (legacy compatibility)
            var config2FA = await _usuarioRepo.Get2FAAsync(usuario.Id);
            if (config2FA is null)
            {
                await _usuarioRepo.Create2FAAsync(new Usuario2FA
                {
                    UsuarioId = usuario.Id,
                    SecretKey = secretKey,
                    Habilitado = false
                });
            }
            else
            {
                config2FA.SecretKey = secretKey;
                config2FA.Habilitado = false;
                await _usuarioRepo.Update2FAAsync(config2FA);
            }

            var qrUrl = $"otpauth://totp/BLL-Construccion:{usuario.NombreUsuario}?secret={secretKey}&issuer=BLL-Construccion";

            return (true, "Escanea el código QR con tu app de autenticación.", new Enable2FAResponseDto
            {
                SecretKey = secretKey,
                QrCodeUrl = qrUrl
            });
        }

        // Método EMAIL → enviar código
        var (permitido, mensajeError) = _rateLimitHelper.PuedeEnviarCodigo($"email_{usuario.Email}");
        if (!permitido)
            return (false, mensajeError!, null);

        // Invalidar códigos anteriores
        await _usuarioRepo.InvalidarCodigosAnterioresAsync(usuario.Id, "email");

        // Generar y guardar nuevo código
        var codigo = OtpHelper.GenerarCodigo();
        var codigoHash = OtpHelper.HashCodigo(codigo, usuario.Email);

        var emailCode = new MfaEmailCode
        {
            UsuarioId = usuario.Id,
            CodigoHash = codigoHash,
            Canal = "email",
            FechaCreacion = DateTime.UtcNow,
            FechaExpira = DateTime.UtcNow.AddMinutes(10),
            IpSolicitud = ipOrigen
        };
        await _usuarioRepo.CreateMfaEmailCodeAsync(emailCode);

        // Enviar email
        var enviado = await _emailService.SendMfaCodeAsync(usuario.Email, usuario.Nombre, codigo, 10);
        if (!enviado)
            return (false, "Error al enviar el código por email. Intenta de nuevo.", null);

        return (true, "Código enviado a tu correo. Verifica tu bandeja de entrada.", null);
    }

    // ─── SEND MFA EMAIL CODE ──────────────────────────────────────────────────
    public async Task<(bool Success, string Message)> SendMfaEmailCodeAsync(
        SendMfaEmailCodeRequestDto dto, string ipOrigen)
    {
        var (valid, ticket) = _mfaTicketHelper.ValidarMfaTicket(dto.MfaTicket);
        if (!valid || ticket is null)
            return (false, "Ticket MFA inválido o expirado.");

        var usuario = await _usuarioRepo.GetByIdAsync(ticket.UsuarioId);
        if (usuario is null)
            return (false, "Usuario no encontrado.");

        var (permitido, mensajeError) = _rateLimitHelper.PuedeEnviarCodigo($"email_{usuario.Email}");
        if (!permitido)
            return (false, mensajeError!);

        // Invalidar códigos anteriores
        await _usuarioRepo.InvalidarCodigosAnterioresAsync(usuario.Id, "email");

        // Generar y guardar nuevo código
        var codigo = OtpHelper.GenerarCodigo();
        var codigoHash = OtpHelper.HashCodigo(codigo, usuario.Email);

        var emailCode = new MfaEmailCode
        {
            UsuarioId = usuario.Id,
            CodigoHash = codigoHash,
            Canal = "email",
            FechaCreacion = DateTime.UtcNow,
            FechaExpira = DateTime.UtcNow.AddMinutes(10),
            IpSolicitud = ipOrigen
        };
        await _usuarioRepo.CreateMfaEmailCodeAsync(emailCode);

        // Enviar email
        var enviado = await _emailService.SendMfaCodeAsync(usuario.Email, usuario.Nombre, codigo, 10);
        if (!enviado)
            return (false, "Error al enviar el código por email. Intenta de nuevo.");

        return (true, "Código enviado a tu correo.");
    }

    // ─── VERIFY MFA ───────────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, LoginResponseDto? Data)> VerifyMfaAsync(
        VerifyMfaRequestDto dto, string ipOrigen)
    {
        var (valid, ticket) = _mfaTicketHelper.ValidarMfaTicket(dto.MfaTicket);
        if (!valid || ticket is null)
            return (false, "Ticket MFA inválido o expirado.", null);

        var usuario = await _usuarioRepo.GetByIdAsync(ticket.UsuarioId);
        if (usuario is null)
            return (false, "Usuario no encontrado.", null);

        var metodo = dto.Metodo.ToLower();

        if (metodo == "app")
        {
            var config2FA = await _usuarioRepo.Get2FAAsync(usuario.Id);
            if (config2FA is null || !config2FA.Habilitado)
                return (false, "MFA por app no está configurado.", null);

            if (!ValidarCodigo2FA(config2FA.SecretKey, dto.Codigo))
            {
                await _usuarioRepo.RegistrarLogAsync(new LogAcceso
                {
                    UsuarioId = usuario.Id,
                    Fecha = DateTime.UtcNow,
                    Exitoso = false,
                    IpOrigen = ipOrigen,
                    Descripcion = "Código MFA app incorrecto"
                });
                return (false, "Código incorrecto.", null);
            }
        }
        else if (metodo == "email")
        {
            var codigoValido = await _usuarioRepo.GetMfaEmailCodeValidoAsync(usuario.Id);
            if (codigoValido is null)
                return (false, "No hay código válido. Solicita uno nuevo.", null);

            if (!OtpHelper.VerificarCodigo(dto.Codigo, codigoValido.CodigoHash, usuario.Email))
            {
                await _usuarioRepo.RegistrarIntentoFallidoAsync(codigoValido.Id);
                await _usuarioRepo.RegistrarLogAsync(new LogAcceso
                {
                    UsuarioId = usuario.Id,
                    Fecha = DateTime.UtcNow,
                    Exitoso = false,
                    IpOrigen = ipOrigen,
                    Descripcion = "Código MFA email incorrecto"
                });
                return (false, "Código incorrecto.", null);
            }

            // Marcar código como usado
            codigoValido.Usado = true;
            codigoValido.IpVerificacion = ipOrigen;
            await _usuarioRepo.MarcarMfaEmailCodigoUsadoAsync(codigoValido);
        }
        else
        {
            return (false, "Método no válido.", null);
        }

        // MFA exitoso → generar tokens
        var response = await GenerarTokensAsync(usuario, ipOrigen);
        return (true, "Verificación MFA exitosa.", response);
    }

    // ─── CONFIRM MFA APP SETUP ────────────────────────────────────────────────
    public async Task<(bool Success, string Message, Enable2FAResponseDto? Data)> ConfirmMfaAppSetupAsync(
        ConfirmMfaAppSetupRequestDto dto)
    {
        var (valid, ticket) = _mfaTicketHelper.ValidarMfaTicket(dto.MfaTicket);
        if (!valid || ticket is null)
            return (false, "Ticket MFA inválido o expirado.", null);

        var usuario = await _usuarioRepo.GetByIdAsync(ticket.UsuarioId);
        if (usuario is null)
            return (false, "Usuario no encontrado.", null);

        var config2FA = await _usuarioRepo.Get2FAAsync(usuario.Id);
        if (config2FA is null)
            return (false, "Primero configura el método app.", null);

        if (!ValidarCodigo2FA(config2FA.SecretKey, dto.CodigoTotp))
            return (false, "Código incorrecto. Intenta de nuevo.", null);

        // Activar 2FA legacy
        config2FA.Habilitado = true;
        config2FA.FechaActivado = DateTime.UtcNow;
        await _usuarioRepo.Update2FAAsync(config2FA);

        // Activar o crear MfaConfig
        var mfaConfig = await _usuarioRepo.GetMfaConfigAsync(usuario.Id);
        if (mfaConfig is null)
        {
            mfaConfig = new UsuarioMfaConfig
            {
                UsuarioId = usuario.Id,
                MfaHabilitado = true,
                MfaMetodoPreferido = "app",
                MfaAppHabilitado = true,
                MfaEmailHabilitado = false,
                MfaUltimaActualizacion = DateTime.UtcNow
            };
            await _usuarioRepo.CreateMfaConfigAsync(mfaConfig);
        }
        else
        {
            mfaConfig.MfaHabilitado = true;
            mfaConfig.MfaMetodoPreferido = "app";
            mfaConfig.MfaAppHabilitado = true;
            mfaConfig.MfaUltimaActualizacion = DateTime.UtcNow;
            await _usuarioRepo.UpdateMfaConfigAsync(mfaConfig);
        }

        return (true, "MFA por app activado correctamente.", null);
    }

    // ─── CHANGE MFA METHOD ────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, Enable2FAResponseDto? Data)> ChangeMfaMethodAsync(
        ChangeMfaMethodRequestDto dto, int usuarioId, string ipOrigen)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId);
        if (usuario is null)
            return (false, "Usuario no encontrado.", null);

        // Verificar password actual
        if (!VerifyPassword(dto.PasswordActual, usuario.PasswordHash))
            return (false, "Contraseña incorrecta.", null);

        var mfaConfig = await _usuarioRepo.GetMfaConfigAsync(usuarioId);
        if (mfaConfig is null || !mfaConfig.MfaHabilitado)
            return (false, "No tienes MFA configurado.", null);

        // Verificar código del método actual
        if (mfaConfig.MfaMetodoPreferido == "app")
        {
            var config2FA = await _usuarioRepo.Get2FAAsync(usuarioId);
            if (config2FA is null || !ValidarCodigo2FA(config2FA.SecretKey, dto.MetodoActualCodigo))
                return (false, "Código del método actual incorrecto.", null);
        }
        else if (mfaConfig.MfaMetodoPreferido == "email")
        {
            var codigoValido = await _usuarioRepo.GetMfaEmailCodeValidoAsync(usuarioId);
            if (codigoValido is null || !OtpHelper.VerificarCodigo(dto.MetodoActualCodigo, codigoValido.CodigoHash, usuario.Email))
                return (false, "Código del método actual incorrecto.", null);
        }

        var metodoNuevo = dto.MetodoNuevo.ToLower();
        if (metodoNuevo != "app" && metodoNuevo != "email")
            return (false, "Método no válido.", null);

        // Cambiar a APP
        if (metodoNuevo == "app")
        {
            var secretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
            var config2FA = await _usuarioRepo.Get2FAAsync(usuarioId);

            if (config2FA is null)
            {
                await _usuarioRepo.Create2FAAsync(new Usuario2FA
                {
                    UsuarioId = usuarioId,
                    SecretKey = secretKey,
                    Habilitado = true,
                    FechaActivado = DateTime.UtcNow
                });
            }
            else
            {
                config2FA.SecretKey = secretKey;
                config2FA.Habilitado = true;
                config2FA.FechaActivado = DateTime.UtcNow;
                await _usuarioRepo.Update2FAAsync(config2FA);
            }

            mfaConfig.MfaMetodoPreferido = "app";
            mfaConfig.MfaAppHabilitado = true;
            mfaConfig.MfaUltimaActualizacion = DateTime.UtcNow;
            await _usuarioRepo.UpdateMfaConfigAsync(mfaConfig);

            var qrUrl = $"otpauth://totp/BLL-Construccion:{usuario.NombreUsuario}?secret={secretKey}&issuer=BLL-Construccion";

            return (true, "Método cambiado a app. Escanea el nuevo QR.", new Enable2FAResponseDto
            {
                SecretKey = secretKey,
                QrCodeUrl = qrUrl
            });
        }

        // Cambiar a EMAIL
        mfaConfig.MfaMetodoPreferido = "email";
        mfaConfig.MfaEmailHabilitado = true;
        mfaConfig.MfaUltimaActualizacion = DateTime.UtcNow;
        await _usuarioRepo.UpdateMfaConfigAsync(mfaConfig);

        return (true, "Método cambiado a email correctamente.", null);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PASSWORD RESET
    // ═══════════════════════════════════════════════════════════════════════════

    // ─── REQUEST PASSWORD RESET ───────────────────────────────────────────────
    public async Task<(bool Success, string Message)> RequestPasswordResetAsync(
        RequestPasswordResetDto dto, string ipOrigen)
    {
        // Rate limiting
        var (permitido, mensajeError) = _rateLimitHelper.PuedeEnviarCodigo($"reset_{dto.Email}");
        if (!permitido)
            return (false, mensajeError!);

        var usuario = await _usuarioRepo.GetByEmailAsync(dto.Email);

        // Mensaje genérico por seguridad (no filtrar si email existe)
        if (usuario is null)
            return (true, "Si el correo existe, recibirás un código de recuperación.");

        if (!usuario.Activo)
            return (true, "Si el correo existe, recibirás un código de recuperación.");

        // Invalidar códigos anteriores
        await _usuarioRepo.InvalidarPasswordResetCodigosAsync(usuario.Id);

        // Generar código
        var codigo = OtpHelper.GenerarCodigo();
        var codigoHash = OtpHelper.HashCodigo(codigo, usuario.Email);

        var resetCode = new PasswordResetCode
        {
            UsuarioId = usuario.Id,
            CodigoHash = codigoHash,
            FechaCreacion = DateTime.UtcNow,
            FechaExpira = DateTime.UtcNow.AddMinutes(15),
            IpSolicitud = ipOrigen
        };
        await _usuarioRepo.CreatePasswordResetCodeAsync(resetCode);

        // Enviar email
        var enviado = await _emailService.SendPasswordResetCodeAsync(usuario.Email, usuario.Nombre, codigo, 15);

        // Mensaje genérico aunque falle el envío
        return (true, "Si el correo existe, recibirás un código de recuperación.");
    }

    // ─── VERIFY PASSWORD RESET CODE ───────────────────────────────────────────
    public async Task<(bool Success, string Message)> VerifyPasswordResetCodeAsync(
        VerifyPasswordResetDto dto, string ipOrigen)
    {
        var usuario = await _usuarioRepo.GetByEmailAsync(dto.Email);
        if (usuario is null)
            return (false, "Código inválido o expirado.");

        var codigoValido = await _usuarioRepo.GetPasswordResetCodeValidoAsync(usuario.Id);
        if (codigoValido is null)
            return (false, "Código inválido o expirado.");

        if (!OtpHelper.VerificarCodigo(dto.Codigo, codigoValido.CodigoHash, usuario.Email))
        {
            await _usuarioRepo.RegistrarIntentoFallidoPasswordResetAsync(codigoValido.Id);
            return (false, "Código incorrecto.");
        }

        return (true, "Código verificado. Puedes cambiar tu contraseña.");
    }

    // ─── CONFIRM PASSWORD RESET ───────────────────────────────────────────────
    public async Task<(bool Success, string Message)> ConfirmPasswordResetAsync(
        ConfirmPasswordResetDto dto, string ipOrigen)
    {
        var usuario = await _usuarioRepo.GetByEmailAsync(dto.Email);
        if (usuario is null)
            return (false, "Código inválido o expirado.");

        var codigoValido = await _usuarioRepo.GetPasswordResetCodeValidoAsync(usuario.Id);
        if (codigoValido is null)
            return (false, "Código inválido o expirado.");

        if (!OtpHelper.VerificarCodigo(dto.Codigo, codigoValido.CodigoHash, usuario.Email))
        {
            await _usuarioRepo.RegistrarIntentoFallidoPasswordResetAsync(codigoValido.Id);
            return (false, "Código incorrecto.");
        }

        // Cambiar contraseña
        usuario.PasswordHash = HashPassword(dto.NuevaPassword);
        await _usuarioRepo.UpdateAsync(usuario);

        // Marcar código como usado
        codigoValido.Usado = true;
        codigoValido.IpVerificacion = ipOrigen;
        await _usuarioRepo.InvalidarPasswordResetCodigosAsync(usuario.Id);

        // Revocar todos los refresh tokens
        await _usuarioRepo.RevocarTodosLosTokensAsync(usuario.Id);

        // Registrar en bitácora
        await _usuarioRepo.RegistrarLogAsync(new LogAcceso
        {
            UsuarioId = usuario.Id,
            Fecha = DateTime.UtcNow,
            Exitoso = true,
            IpOrigen = ipOrigen,
            Descripcion = "Contraseña restablecida"
        });

        return (true, "Contraseña restablecida correctamente. Inicia sesión con tu nueva contraseña.");
    }

    // ─── HELPERS PRIVADOS ─────────────────────────────────────────────────────
    private async Task<LoginResponseDto> GenerarTokensAsync(Usuario usuario, string ipOrigen)
    {
        var accessToken = GenerarJWT(usuario);
        var refreshToken = GenerarRefreshToken();
        var expira = DateTime.UtcNow.AddMinutes(
            int.Parse(_config["Jwt:AccessTokenMinutes"]!));

        await _usuarioRepo.CreateTokenAsync(new TokenSesion
        {
            UsuarioId = usuario.Id,
            Token = refreshToken,
            FechaExpira = DateTime.UtcNow.AddMinutes(
                int.Parse(_config["Jwt:RefreshTokenMinutes"]!)),
            Revocado = false,
            FechaCreacion = DateTime.UtcNow,
            IpOrigen = ipOrigen
        });

        usuario.UltimoAcceso = DateTime.UtcNow;
        await _usuarioRepo.UpdateAsync(usuario);

        await _usuarioRepo.RegistrarLogAsync(new LogAcceso
        {
            UsuarioId = usuario.Id,
            Fecha = DateTime.UtcNow,
            Exitoso = true,
            IpOrigen = ipOrigen,
            Descripcion = "Login exitoso"
        });

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expira = expira,
            Requiere2FA = false,
            UsuarioId = usuario.Id,
            NombreUsuario = usuario.NombreUsuario
        };
    }

    private string GenerarJWT(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenMinutes"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("nombreUsuario",               usuario.NombreUsuario),
            new Claim("rolId",                       usuario.RolId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerarRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    private static bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);

    private static bool ValidarCodigo2FA(string secretKey, string codigo)
    {
        var keyBytes = Base32Encoding.ToBytes(secretKey);
        var totp = new Totp(keyBytes);
        return totp.VerifyTotp(codigo, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }
}