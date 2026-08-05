using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedListadoPersonalHoja2 : Migration
    {
        // No. Empleado, Nombre, Puesto, CURP, RFC, NSS, Telefono, FechaIngreso, Estatus, SueldoNetoSemanal, CreditoInfonavit, TipoDescuentoInfonavit, CuotaInfonavit
        // Origen: BASE DE DATOS BLL.xlsx, Hoja2 (LISTADO DE PERSONAL). Cada INSERT es idempotente
        // (solo inserta si el No. de Empleado no existe ya), para que un redeploy o una carga manual
        // previa nunca truene la migración.
        private static readonly (string Num, string Nombre, string Puesto, string Curp, string Rfc, string Nss, string Tel, string Fecha, string Estatus, decimal? Sueldo, bool Credito, string TipoDesc, decimal? Cuota)[] Empleados =
        {
            ("OFL001", "BALDEMAR LOPEZ LOPEZ", "CEO", "LOLB931003HVZPPL09", "LOLB931003", "35-14-93-5196-3", "81 2578 7691", null, "Activo", null, false, null, null),
            ("OFL002", "IVAN SANTAMARIA", "PROGRAMADOR", null, null, null, "866 185 3120", null, "Activo", null, false, null, null),
            ("OFL003", "JESUS MANUEL SALAZAR HERNANDEZ", "ELECTRICO", "SAHJ891110HCLLRS09", "SAHJ891110", "32-07-89-0983-8", "866 148 9073", null, "Activo", 5000, true, "CUOTA FIJA", null),
            ("ADM001", "VANNIA DIONISIO ACOSTA", "ASISTENTE", "DIAV901010MDFNCN13", "DIAV901010K31", "19-17-96-5842-2", "56 1429 4527", "2025-07-07", "Activo", 2500, false, null, null),
            ("ADM002", "VANESSA OLAYA REYES", "ASISTENTE", "OARV021220MVZLYNA2", "OARV021220", "51-60-26-1193-0", "56 1826 5936", null, "Activo", 2000, false, null, null),
            ("OPR001", "OSCAR GRANADOS LAZARO", "SOLDADOR", "GALO730203HDFRZS07", "GALO730203", "43947377065", "81 3276 2068", null, "Activo", 6000, true, "CUOTA FIJA", null),
            ("OPR002", "CESAR GERARDO LOPEZ LOPEZ", "ELECTRICO", "LOLC891223HVZPPS12", "LOLC891223", "43-08-89-3240-5", "81 1215 3646", null, "Activo", 4000, false, null, null),
            ("OPR003", "CARLOS ALBERTO DIAZ RUIZ", "MECANICO", null, null, "43-94-79-5895-5", null, null, "Baja", 5000, false, null, null),
            ("OPR004", "ESTEBAN ALONSO GARCIA", "ELECTRICO", null, null, "65-10-90-2608-1", null, null, "Baja", 2500, false, null, null),
            ("OPR005", "MOISES ALONSO GARCIA", "AYUDANTE GENERAL", null, null, "17-25-06-8893-8", null, null, "Baja", 2500, false, null, null),
            ("OPR006", "DIEGO RODRIGUEZ ESQUIVEL", "SOLDADOR", "ROED840927HCLDSG01", "ROED840927B35", "32-00-81-2872-3", "866 144 8734", "2025-05-01", "Activo", 6000, false, null, null),
            ("OPR010", "ABEL OLAYA REYES", "AYUDANTE GENERAL", "OARA060928HVZLYBA9", "OARA060928", "44-22-06-7611-6", "232 162 0908", "2025-02-01", "Activo", 2500, false, null, null),
            ("OPR011", "RAFAEL MEZA YESCAS", "AYUDANTE GENERAL", null, null, null, null, null, "Baja", 2500, false, null, null),
            ("OPR012", "MARCO URIEL MECATL GONZALEZ", "SOLDADOR", null, null, null, null, null, "Baja", 3300, false, null, null),
            ("OPR014", "RUBEN LEMUS BAEZ", "AYUDANTE GENERAL", null, null, null, null, null, "Baja", 2500, false, null, null),
            ("OPR016", "JONATHAN ROJAS HERNANDEZ", "ELECTRICO", "ROHJ010310HMCJRNA0", "ROHJ010310235", "46-16-01-8407-6", "984 538 7066", "2025-08-05", "Activo", 4500, false, null, null),
            ("OPR017", "HUGO GARCIA GARCIA", "SOLDADOR", "GAGH860923HMCRRG03", "GAGH860923QH5", "13-05-86-1908-4", "55 2669 9130", "2025-08-05", "Activo", 4500, false, null, null),
            ("OPR018", "ARNULFO ISMAEL MARTINEZ GARCIA", "ELECTRICO", "MAGA770922HCLRRR00", "MAGA770922", "60-95-77-8285-1", "866 142 9698", null, "Activo", 6000, true, "CUOTA FIJA", null),
            ("OPR019", "ANGEL ANTONIO ESMERALDA RAMIREZ", "ELECTRICO", "EERA900622HNLSMN07", "EERA900622", "43-06-90-1378-7", "81 3102 3032", null, "Activo", 4000, true, "CUOTA FIJA", 2542.47m),
            ("OPR020", "JESUS JAIME MARTINEZ", "ELECTRICO", "JAMJ751108HNLMRS01", "JAMJ751108", "47-93-75-1878-6", "81 2179 1212", "2025-09-09", "Activo", 4500, false, null, null),
            ("OPR021", "VICTOR HUGO PEÑA OCEJO", "ELECTRICO", "PEOV991008HZXCC07", "PEOV991008", "57169992328", "899 316 8007", "2025-09-09", "Baja", 5000, true, "CUOTA FIJA", null),
            ("OPR022", "JOSE OSCAR TONATIHU SANTANA HDZ", "AYUDANTE GENERAL", null, null, null, null, null, "Baja", 2500, false, null, null),
            ("OPR023", "PABLO ESTEBAN MORALES RODRIGUEZ", "AYUDANTE GENERAL", null, null, null, null, null, "Baja", 2500, false, null, null),
            ("OPR024", "ROGELIO LIZARDI CASTRO", "AYUDANTE GENERAL", null, null, null, null, null, "Baja", 2500, false, null, null),
            ("OPR025", "ROBERTO ESPINOZA", "ELECTRICO", null, null, null, null, null, "Baja", 4000, false, null, null),
            ("OPR026", "LUIS ALBERTO LOPEZ", "SOLDADOR", "LOIL710516HCLPSS05", "LOIL710516D99", "32-89-71-3533-4", null, null, "Baja", 6000, false, null, null),
            ("OPR027", "LEONARDO LOPEZ", "ELECTRICO", null, null, null, null, null, "Baja", 3000, false, null, null),
            ("OPR028", "JULIO CORDOVA DIAZ", "ELECTRICO", null, null, null, "56 6254 9349", "2025-11-21", "Activo", 3500, false, null, null),
            ("OPR029", "ERNESTO CARLOS REYES RIVAS", "MECANICO", "RERE750810HCLYVR06", "RERE750810VE5", "33-93-75-1635-0", "866 643 7537", null, "Baja", 6000, false, null, null),
            ("OPR030", "OCTAVIO VANDA RAMIREZ", "ELECTRICO", "BARO740620HSPNMC09", "BARO740620", "02-17-74-7864-7", null, "2026-03-02", "Baja", 5500, false, null, null),
            ("OPR031", "LUCIO LUNA JAVIER ALEJANDRO", "AYUDANTE GENERAL", "LULJ030516HNLCNVA5", null, "30-22-03-1323-0", "81 1590 4057", "2026-03-09", "Baja", 3500, false, null, null),
            ("OPR032", "MARTINEZ RUBALCABAR YAHIR ALEJANDRO", "AYUDANTE GENERAL", "MARY050704HTCRBHA0", "MARY050704R32", "25-22-05-3983-4", "937 122 5518", "2026-03-09", "Baja", 3500, false, null, null),
            ("OPR033", "PECINA SOLIS CARLOS", "SOLDADOR", "PESC731002HNLCLR02", "PESC7310024H7", "47-90-73-0811-9", "81 1845 3141", "2026-03-09", "Baja", 4000, false, null, null),
            ("OPR034", "VAZQUEZ GARCIA SERGIO", "AYUDANTE GENERAL", "VAGS820923HVZZRR07", "VAGS8209239W6", "65-96-82-0540-4", "232 139 8372", "2026-03-09", "Baja", 3500, false, null, null),
            ("OPR035", "VAZQUEZ PECINA FELIPE FLORENTINO", "ELECTRICO", "VAPF800308HNLZCL02", "VAPF800308", "39-68-09-5665", "81 2339 8940", "2026-03-09", "Baja", 6000, true, "CUOTA FIJA", null),
            ("OPR036", "JUAN CARLOS ESPINOZA RUIZ", "SOLDADOR", null, null, null, "55 2851 8392", "2026-03-23", "Activo", 5000, false, null, null),
            ("OPR037", "DANIEL DE LUCIO ROMO", "AYUDANTE GENERAL", null, null, null, "55 1789 8127", "2026-03-23", "Baja", 3500, false, null, null),
            ("OPR038", "JOSE LUIS DE LA LUNA SANTES", "AYUDANTE GENERAL", null, null, null, "766 207 3050", "2026-03-23", "Baja", 3500, false, null, null),
            ("OPR039", "ISMAEL OLMEDO PEREZ", "ELECTRICO", null, null, null, "826 237 3457", "2026-03-23", "Activo", 4500, false, null, null),
            ("OPR040", "JOSE RUBEN LOPEZ RUIZ", "ELECTRICO", null, null, null, null, "2026-03-23", "Baja", 5000, false, null, null),
            ("OPR041", "JOSE ALEJANDRO LOPEZ PEREZ", "SOLDADOR", null, null, null, "55 1009 1231", "2026-03-26", "Baja", 5500, false, null, null),
            ("OPR042", "SERGIO CHILIAN NICOLAT", "ELECTRICO", null, null, null, "81 1674 6514", "2026-03-30", "Activo", 6000, false, null, null),
            ("OPR043", "ROBERTO RAMOS DE LA CRUZ", "SOLDADOR", null, null, null, "866 212 6245", "2026-06-04", "Activo", 5000, false, null, null),
        };

        private static string Lit(string v) => v is null ? "NULL" : "N'" + v.Replace("'", "''") + "'";
        private static string Lit(decimal? v) => v is null ? "NULL" : v.Value.ToString(CultureInfo.InvariantCulture);
        private static string LitDate(string v) => v is null ? "NULL" : "N'" + v + "'";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var e in Empleados)
            {
                var sql = $@"
IF NOT EXISTS (SELECT 1 FROM Empleados WHERE NumeroEmpleado = {Lit(e.Num)})
INSERT INTO Empleados
    (NumeroEmpleado, NombreCompleto, Puesto, CURP, RFC, NSS, Telefono, ContactoEmergencia, Domicilio,
     FechaIngreso, FechaVencimientoContrato, Estatus, SueldoNetoSemanal, CreditoInfonavit,
     TipoDescuentoInfonavit, CuotaInfonavit, Observaciones, FechaRegistro)
VALUES
    ({Lit(e.Num)}, {Lit(e.Nombre)}, {Lit(e.Puesto)}, {Lit(e.Curp)}, {Lit(e.Rfc)}, {Lit(e.Nss)}, {Lit(e.Tel)}, NULL, NULL,
     {LitDate(e.Fecha)}, NULL, {Lit(e.Estatus)}, {Lit(e.Sueldo)}, {(e.Credito ? 1 : 0)},
     {Lit(e.TipoDesc)}, {Lit(e.Cuota)}, NULL, SYSUTCDATETIME());";

                migrationBuilder.Sql(sql);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var nums = new List<string>();
            foreach (var e in Empleados) nums.Add(Lit(e.Num));

            migrationBuilder.Sql($"DELETE FROM Empleados WHERE NumeroEmpleado IN ({string.Join(", ", nums)});");
        }
    }
}
