using ConversorMoedasMVVM.ViewModels;
// Importa o namespace onde está a MainViewModel.
// Assim podemos instanciar a VM aqui dentro da View.

namespace ConversorMoedasMVVM.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        // [1] Método gerado automaticamente pelo XAML.
        // Ele "monta" a interface gráfica descrita no MainPage.xaml.
        // No projeto anterior (code-behind), isso continuava igual.

        BindingContext = new MainViewModel();
        // [2] Aqui está a grande diferença!
        // Define que o contexto de dados (BindingContext) desta página
        // será uma instância da MainViewModel.
        //
        // Isso significa que todos os Bindings do XAML (AmountText, From, To, ResultText, Commands...)
        // vão "conversar" com essa instância.
        //
        // Antes (code-behind):
        //   - Você declarava controles com x:Name (ex.: valorEntry, resultadoLabel).
        //   - E manipulava diretamente no .cs (ex.: resultadoLabel.Text = ...).
        // Agora (MVVM):
        //   - Não precisamos mais de x:Name nos controles.
        //   - Tudo se conecta automaticamente via Binding → VM.
    }
}
