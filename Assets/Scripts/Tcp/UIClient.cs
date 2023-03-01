using UnityEngine;
using UnityEngine.UI;

namespace Tcp
{
    public class UIClient : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private InputField inputField;
        [SerializeField] private InputField ipField;
        [SerializeField] private InputField portField;

        [SerializeField] private Button btnConnect;
        [SerializeField] private Button btnSend;
        [SerializeField] private Button btnClose;
        [SerializeField] private Client client;

        private void OnEnable()
        {
            btnConnect.onClick.AddListener((() =>
            {
                //client.ConnectToServer(ipField.text, int.Parse(portField.text));
                client.ConnectToServer("127.0.0.1", 9000);
            }));
        
            btnSend.onClick.AddListener((() =>
            {
                string content = inputField.text;
                client.SendMessageToServer(content);
            }));
            btnClose.onClick.AddListener((() =>
            {
                client.DisconnectToServer();
            }));
            client.OnStatus = (str =>
            {
                text.text = str;
            });
        }

        private void OnDisable()
        {
            btnSend.onClick.RemoveAllListeners();
            client.OnStatus = null;
        }
    
    }
}
