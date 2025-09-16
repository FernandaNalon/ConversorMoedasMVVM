namespace ConversorMoedasMVVM.Models;

// === MODEL ===
// No padrão MVVM, a Model representa os "dados de domínio"
// e as "regras de negócio". 
// → Aqui: tabela de câmbio fictícia + regra de conversão.
public class RateTable
{
    // [1] Dicionário com as taxas de câmbio
    // Cada chave é um código de moeda (BRL, USD, EUR...).
    // O valor é "quantos BRL vale 1 unidade da moeda".
    // → No projeto anterior (code-behind): isso ficava como 
    // variável dentro do MainPage.xaml.cs.
    private readonly Dictionary<string, decimal> _toBRL = new()
    {
        ["BRL"] = 1.00m, // 1 BRL = 1 BRL (moeda base)
        ["USD"] = 5.60m, // 1 USD = 5,60 BRL
        ["EUR"] = 6.10m  // 1 EUR = 6,10 BRL
    };

    // [2] Propriedade de leitura da tabela
    // Expõe o dicionário como "somente leitura" 
    // para que a ViewModel possa consultar, mas sem permitir alterações diretas.
    public IReadOnlyDictionary<string, decimal> ToBRL => _toBRL;

    // [3] Método para retornar as moedas disponíveis
    // → Usado pela ViewModel para popular o Picker (combobox).
    // → Antes: no code-behind a lista era carregada direto no construtor da View.

    public IEnumerable<string> GetCurrencies()
        => _toBRL.Keys.OrderBy(k => k);
    //IEnumerable<string> → é o tipo de retorno e significa “uma coleção que pode ser percorrida” (foreach ou ItemsSource do Picker).
    //Cada item dessa coleção é uma string (ex.: "BRL", "USD", "EUR").
    //Nome do método: GetCurrencies() → convenção: “Get” = “me dê uma lista de...”.
    //Corpo: _toBRL.Keys.OrderBy(k => k)
    //_toBRL.Keys → retorna todas as chaves do dicionário _toBRL(as moedas cadastradas).
    //.OrderBy(k => k) → ordena em ordem alfabética.
    //k representa cada chave ("BRL", "USD", "EUR").
    //Resultado final: uma lista como["BRL", "EUR", "USD"].



    // [4] Método para verificar se uma moeda existe na tabela
    // Facilita validações antes da conversão.
    public bool Supports(string code) => _toBRL.ContainsKey(code);
    //Nome do método: Supports → “suporta essa moeda?”.
    //Parâmetro: (string code): code é uma string que representa o código da moeda a ser testada.Ex.: "USD", "BRL", "JPY".
    //Corpo: _toBRL.ContainsKey(code): Vai ao dicionário _toBRL e verifica se existe aquela chave. Se existir → true. Se não existir → false.



    // [5] Método de conversão principal
    // Regra: amount * (BRL por FROM) / (BRL por TO).
    // → Antes: essa conta estava diretamente dentro do método do botão "Converter".
    // → Agora: a regra de negócio fica isolada na Model,
    //          deixando a ViewModel só responsável por "pedir o cálculo".
    public decimal Convert(decimal amount, string from, string to)
    {
        if (!Supports(from) || !Supports(to)) return 0m;
        if (from == to) return amount;

        // Converte para BRL como base intermediária
        var brl = amount * _toBRL[from];

        // Converte de BRL para a moeda de destino
        return brl / _toBRL[to];
    }
}
