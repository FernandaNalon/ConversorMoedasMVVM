using ConversorMoedasMVVM.Models;         
using System;
using System.ComponentModel;               
using System.Globalization;                
using System.Runtime.CompilerServices;    
using System.Windows.Input;              

// Note a separação:
// Model (RateTable) → sabe as moedas e como converter.
// ViewModel → guarda o estado da tela e chama a Model.
// View (XAML) → só mostra e faz binding.

namespace ConversorMoedasMVVM.ViewModels;

// Permite que a View “ouça” mudanças de propriedades e atualize a UI automaticamente.
// Antes: você fazia label.Text = ... no Clicked.Agora a View atualiza sozinha via Binding.
public class MainViewModel : INotifyPropertyChanged         
{
    // PropertyChanged + OnPropertyChanged: Padrão para notificar a View. [CallerMemberName]
    // evita escrever o nome da propriedade manualmente.
    // Antes: sem notificação; atualizava os controles imperativamente.
    public event PropertyChangedEventHandler? PropertyChanged;         
    
    void OnPropertyChanged([CallerMemberName] string? name = null)       
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



    private readonly RateTable _rates = new();   
    // A ViewModel não calcula câmbio; delega à Model e importa;
    // Antes: a tabela e a conta estavam no MainPage.xaml.cs.

    public IList<string> Currencies { get; }
    // A lista (para os Pickers) vem de _rates.GetCurrencies().
    // Antes: montada diretamente na View.


    //AmountText é o que o usuário digita no Entry.
    //Ao setar: notifica a View e chama ChangeCanExecute() para reavaliar se o botão Converter pode ser habilitado.
    //Antes: usava TextChanged/Clicked no code-behind para validar/parsing.
    string? _amountText;                         
    public string? AmountText                    
    {
        get => _amountText;
        set
        {
            if (_amountText == value) return;
            _amountText = value;
            OnPropertyChanged();                                         
            ((Command)ConvertCommand).ChangeCanExecute();                
        }
    }

    //Propriedades vinculadas aos Pickers “De” e “Para”.
    //Ao mudar, notificam a View e reavaliam CanConvert.
    //Antes: liam o SelectedItem do Picker direto no evento do botão.
    string _from = "USD";                        
    public string From
    {
        get => _from;
        set
        {
            if (_from == value) return;
            _from = value;
            OnPropertyChanged();                                         
            ((Command)ConvertCommand).ChangeCanExecute();                
        }
    }

    string _to = "BRL";                          
    public string To
    {
        get => _to;
        set
        {
            if (_to == value) return;
            _to = value;
            OnPropertyChanged();                                        
            ((Command)ConvertCommand).ChangeCanExecute();               
        }
    }

    // Texto exibido no Label de resultado; muda via OnPropertyChanged.
    //Antes: resultadoLabel.Text = ... no code-behind.
    string _resultText = "—";                    
    public string ResultText
    {
        get => _resultText;
        set { if (_resultText != value) { _resultText = value; OnPropertyChanged(); } }   
    }

    // ConvertCommand, SwapCommand, ClearCommand substituem Click handlers da View.
    // Antes: button.Clicked += ... no.xaml.cs.
    public ICommand ConvertCommand { get; }      
    public ICommand SwapCommand { get; }       
    public ICommand ClearCommand { get; }


    // Garante parsing/format com vírgula e formatação numérica N2.
    // Antes: parsing ficava espalhado no botão; sujeito a erros de cultura.
    readonly CultureInfo _pt = new("pt-BR");


    // Carrega Currencies e cria os Commands.
    // Antes: fazia muito no construtor da View(misturando UI + regra).
    public MainViewModel()                       
    {
        Currencies = _rates.GetCurrencies().ToList();    

        ConvertCommand = new Command(DoConvert, CanConvert);  
        SwapCommand = new Command(DoSwap);                 
        ClearCommand = new Command(DoClear);               
    }

    // Define quando o botão Converter está habilitado.
    // Depende de AmountText ser válido.
    // Antes: botão sempre habilitado; só no Clicked aparecia erro.
    bool CanConvert()                             
    {
        if (string.IsNullOrWhiteSpace(AmountText)) return false;
        return TryParseAmount(AmountText, out _);
    }

    // Faz parsing seguro; valida moedas; chama Model.Convert; formata resultado.
    // Antes: tudo isso ficava dentro do evento do botão.
    void DoConvert()                              
    {
        if (!TryParseAmount(AmountText, out var amount))
        {
            ResultText = "Valor inválido.";
            return;
        }
        if (!_rates.Supports(From) || !_rates.Supports(To))
        {
            ResultText = "Moeda não suportada.";
            return;
        }
        var result = _rates.Convert(amount, From, To);   //chama a Model
        ResultText = string.Format(_pt, "{0:N2} {1} = {2:N2} {3}", amount, From, result, To);
        // Formatação: quantia + moeda + "=" + quantia convertida + moeda
    }

    // Troca From e To; reseta ResultText.
    // Antes: manipulação direta dos Pickers no code-behind.
    void DoSwap()                                  
    {
        (From, To) = (To, From);
        ResultText = "—";
    }

    // Zera entrada e resultado.
    // Antes: limpava os controles manualmente na View.
    
        void DoClear()                                
    {
        AmountText = string.Empty;
        ResultText = "—";
    }

    // Tenta converter o texto para decimal; aceita , ou ..
    // Antes: parsing ad-hoc no botão; sujeito a inconsistência.
    bool TryParseAmount(string? text, out decimal amount)     
    {
        amount = 0m;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var s = text.Trim(); // Trim() remove espaços extras do início e do fim.
        if (decimal.TryParse(s, NumberStyles.Number, _pt, out amount)) return true;

        s = s.Replace(".", ","); // Troca os pontos por vírgulas e tenta de novo.
        return decimal.TryParse(s, NumberStyles.Number, _pt, out amount);
    }
}
