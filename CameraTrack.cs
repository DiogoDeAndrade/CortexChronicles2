using OpenTK;
using OpenTK.Mathematics;
using System;

namespace OpenTKBase
{
    public class CameraTrack : Component
    {
        public PlayerBike       player;
        private Vector3         offset = new Vector3(0.0f, 2.5f, -5.0f);
        private float           followSpeed = 0.55f;
        public GameMap          gameMap;
        public bool             reset = true;

        public override void Start()
        {
        }

        public override void Update()
        {
            Vector3 targetPos = player.transform.position + 
                                offset.X * player.transform.right +
                                offset.Y * player.transform.up +
                                offset.Z * player.transform.forward;

            if (reset)
            {
                transform.position = targetPos;
                transform.rotation = player.transform.rotation;
            }
            else
            {
                Vector3 currentPos = transform.position;
                Vector3 newPos = currentPos + (targetPos - currentPos) * followSpeed;

                if (gameMap != null)
                {
                    Vector3 rayDir = currentPos - newPos;
                    float maxDist = rayDir.Length;
                    if (maxDist > 0)
                    {
                        rayDir /= maxDist;
                        maxDist += 0.1f;
                        float dist = 0.0f;
                        Vector3 normal = Vector3.Zero;
                        if (gameMap.Raycast(currentPos, rayDir, maxDist, ref dist, ref normal))
                        {
                            newPos = currentPos + rayDir * maxDist + normal * 1.0f;
                        }
                    }
                }

                transform.position = newPos;
                transform.rotation = player.transform.rotation;
            }
        }
    }
}
