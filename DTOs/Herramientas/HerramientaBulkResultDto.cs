namespace BLL_ConstruccionAPI.DTOs.Herramientas;

public class HerramientaBulkResultDto
{
    public string Codigo { get; set; } = string.Empty;
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int? Id { get; set; }
}
