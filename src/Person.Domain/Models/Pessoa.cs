using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Person.Domain.Models;

public class Pessoa
{
    protected Pessoa() { }

    private Pessoa(string primeiroNome, string segundoNome, int idade, DateTime dataCadastro, decimal salario, bool isActive)
    {
        PrimeiroNome = primeiroNome;
        SegundoNome = segundoNome;
        Idade = idade;
        DataCadastro = dataCadastro;
        Salario = salario;
        IsActive = isActive;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string PrimeiroNome { get; private set; }
    public string SegundoNome { get; private set; }
    public int Idade { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public decimal Salario { get; private set; }
    public bool IsActive { get; private set; }

    public static Pessoa Create(string primeiroNome, string segundoNome, int idade, DateTime dataCadastro, decimal salario, bool isActive)
        => new Pessoa(primeiroNome, segundoNome, idade, dataCadastro, salario, isActive);
}
