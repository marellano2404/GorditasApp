namespace GorditasApp.Models;

public class ItemPedido
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public TipoAlimento Tipo { get; set; }
    public TipoRelleno Relleno { get; set; }
    public int Cantidad { get; set; } = 1;

    public string Descripcion =>
        $"{Cantidad} {(Tipo == TipoAlimento.Gordita ? "Gordita(s)" : "Taco(s)")} de {Relleno}";
}
