using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenAiWPF
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HOTKEY_ID = 9000;

    //Modifiers:
    private const uint MOD_NONE = 0x0000; //(none)
    private const uint MOD_ALT = 0x0001; //ALT
    private const uint MOD_CONTROL = 0x0002; //CTRL
    private const uint MOD_SHIFT = 0x0004; //SHIFT
    private const uint MOD_WIN = 0x0008; //WINDOWS

    private string _lastInput = "";

    //CAPS LOCK:
    private const uint VK_CAPITAL = 0x46;

    public MainWindow()
    {
      InitializeComponent();
    }

    private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        // Handle the Enter key press here...

        // Make the spinner visible while the REST call is in progress
        loadingSpinner.Visibility = Visibility.Visible;

        //... REST CALL
        string url = "https://api.openai.com/v1/completions";

        // Set the bearer token to include in the request header
        string bearerToken = "";

        // Create a new HttpClient and set the authorization header
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        client.DefaultRequestHeaders.Add("OpenAI-Organization", "org-DA0gFR30vDvgnp5dAx1Enj3B");
        //client.DefaultRequestHeaders.Add("Content-Type", "application/json");

        var requestObject = new
        {
          model = "text-davinci-003",
          temperature = 0.7,
          max_tokens = 256,
          top_p = 1,
          frequency_penalty = 0,
          presence_penalty = 0,
          prompt = textBox.Text
        };


        // serialize the request object into a JSON string
        var requestContent = JsonConvert.SerializeObject(requestObject);

        // create the request content and set the content type
        var content = new StringContent(requestContent, Encoding.UTF8, "application/json");


        // send the request and wait for the response
        var response = client.PostAsync(url, content).Result;

        // read the response content
        var responseContent = response.Content.ReadAsStringAsync().Result;

        var responseObject = JsonConvert.DeserializeObject(responseContent);

        var o = JObject.Parse(responseContent);

        var choices = o.GetValue("choices");
        var first = choices.First();
        var text = first.SelectToken("text");
        textBlock.Text = text.ToString();


        loadingSpinner.Visibility = Visibility.Collapsed;

        var i = 0;
        _lastInput = textBox.Text;
        textBox.Clear();
      }
    }

    private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.Up)
      {
        // Handle the Enter key press here...
        textBox.Text = _lastInput.Length > 0 ? _lastInput : textBox.Text;
        textBox.SelectionStart = textBox.Text.Length;
        textBox.SelectionLength = 0;
      }
    }

    private IntPtr _windowHandle;
    private HwndSource _source;
    protected override void OnSourceInitialized(EventArgs e)
    {
      base.OnSourceInitialized(e);

      _windowHandle = new WindowInteropHelper(this).Handle;
      _source = HwndSource.FromHwnd(_windowHandle);
      _source.AddHook(HwndHook);

      RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_CAPITAL); //CTRL + CAPS_LOCK
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      const int WM_HOTKEY = 0x0312;
      switch (msg)
      {
        case WM_HOTKEY:
          switch (wParam.ToInt32())
          {
            case HOTKEY_ID:
              int vkey = (((int)lParam >> 16) & 0xFFFF);
              if (vkey == VK_CAPITAL)
              {
                var i = 0;
                Activate();
                textBox.Focus();

                //tblock.Text += "CapsLock was pressed" + Environment.NewLine;
              }
              handled = true;
              break;
          }
          break;
      }
      return IntPtr.Zero;
    }

    protected override void OnClosed(EventArgs e)
    {
      _source.RemoveHook(HwndHook);
      UnregisterHotKey(_windowHandle, HOTKEY_ID);
      base.OnClosed(e);
    }

    private void textBox_TextChanged(object sender, TextChangedEventArgs e)
    {

    }
  }
}
