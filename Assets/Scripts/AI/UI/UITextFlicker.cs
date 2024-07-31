using System.Collections;
using TMPro;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>UITextFlicker</c>
    /// </summary>
    public class UITextFlicker : MonoBehaviour
    {
        private GameManager m_GameManager;          // Reference to GameManager.
        private TextMeshProUGUI m_InfoText;         // Reference to the InfoText.
        private string m_TextContent;               // Content of the InfoText.
        private WaitForSeconds m_WaitForSeconds;    // How many seconds to wait.

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            m_InfoText = GetComponent<TextMeshProUGUI>();
            m_TextContent = m_InfoText.text;
        }

        /// <summary>
        /// Method <c>Start</c> is called before the first frame update.
        /// </summary>
        private void Start()
        {
            m_GameManager = GameManager.Instance; // Do not move it to Awake() because it will be null.
            m_WaitForSeconds = new WaitForSeconds(m_GameManager.InfoBlinkInterval);

            StartCoroutine(InfoFlicker());
        }

        /// <summary>
        /// Coroutine <c>InfoFlicker</c> is used to make the InfoText flicker.
        /// </summary>
        private IEnumerator InfoFlicker()
        {
            while(true)
            {
                m_InfoText.text = m_TextContent;
                yield return m_WaitForSeconds;
                m_InfoText.text = "";
                yield return m_WaitForSeconds;
            }
        }
    }
}
