using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Scripts
{
    public class MapTransitions : MonoBehaviour
    {
        [SerializeField] PolygonCollider2D mapBoundry;
        CinemachineConfiner confiner;
        [SerializeField] Direction direction;

        enum Direction { Up, Down, Left, Right }

        private void Awake()
        {
            confiner = FindFirstObjectByType<CinemachineConfiner>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                confiner.m_BoundingShape2D = mapBoundry;
                UpdatePlayerPosition(collision.gameObject);
            }
        }

        private void UpdatePlayerPosition(GameObject player)
        {
            Vector3 newPos = player.transform.position;

            switch (direction)
            {
                case Direction.Up:
                    newPos.y += 12;
                    break;
                case Direction.Down: 
                    newPos.y -= 12;
                    break;
                case Direction.Left:
                    newPos.x += 12;
                    break;
                case Direction.Right:
                    newPos.x -= 12;
                    break;
            }

            player.transform.position = newPos;
        }
    }
}
