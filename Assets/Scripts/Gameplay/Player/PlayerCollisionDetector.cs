using Managers;
using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// PlayerCollisionDetector : Déclenche un game over si le joueur percute
    /// un mur ou un chariot NPC. Ajoute les tags "Wall" et "NPC" aux objets
    /// concernés dans l'Inspector Unity.
    /// </summary>
    public class PlayerCollisionDetector : MonoBehaviour
    {
        [SerializeField] private string[] dangerTags = { "Wall", "NPC" };

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

            foreach (string tag in dangerTags)
            {
                if (collision.gameObject.CompareTag(tag))
                {
                    GameManager.Instance.OnAccident();
                    return;
                }
            }
        }
    }
}
