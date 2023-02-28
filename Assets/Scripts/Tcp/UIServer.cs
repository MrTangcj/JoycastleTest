using UnityEngine;
using UnityEngine.UI;

namespace Tcp
{
   public class UIServer : MonoBehaviour
   {
      [SerializeField] private Text text;
      [SerializeField] private InputField inputField;
      [SerializeField] private Button btnSend;
      [SerializeField] private Server server;

      private void OnEnable()
      {
         btnSend.onClick.AddListener((() =>
         {
            string content = inputField.text;
            server.SendMessageToClient("127.0.0.1", content);
         }));
         server.OnStatus = (str => text.text += str);
      }

      private void OnDisable()
      {
         btnSend.onClick.RemoveAllListeners();
         server.OnStatus = null;
      }
   }
}
