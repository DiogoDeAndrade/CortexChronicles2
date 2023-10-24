using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Buffers.Text;

namespace OpenTKBase
{
    public class Drone : Component
    {
        private enum State { Idle, Attack };

        private State           state;
        private SpriteRenderer  spriteRenderer;
        private SpriteRenderer  shadowRenderer;
        private SpriteRenderer  targetObjectRenderer;
        private float           baseY;
        private PlayerBike      player;
        private float           distanceToTarget = 0.0f;
        private float           attackCooldown = 3.0f;
        private float           lockTime =  2.0f;
        private float           shootCooldown = 1.5f;
        private int             shotsPerAttack = 3;

        private float           attackTimer;
        private float           lockTimer;
        private float           lockPos;
        private float           shootTimer;
        private int             currentShots;
        private Vector3         startPosition;

        public override void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.mode = SpriteRenderer.Mode.Billboard;

            baseY = transform.position.Y;

            GameObject playerShadow = new GameObject();
            playerShadow.transform.SetParent(transform);
            playerShadow.transform.position = transform.position.X0Z();
            shadowRenderer = playerShadow.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = Resources.FindSprite("DroneShadow");
            shadowRenderer.mode = SpriteRenderer.Mode.BillboardXZ;

            GameObject targetObject = new GameObject();
            targetObject.transform.position = transform.position.X0Z();
            targetObjectRenderer = targetObject.AddComponent<SpriteRenderer>();
            targetObjectRenderer.sprite = Resources.FindSprite("DroneTarget");
            targetObjectRenderer.mode = SpriteRenderer.Mode.BillboardXZ;

            startPosition = transform.position;
        }

        public override void Update()
        {
            if (player == null)
            {
                player = FindObjectOfType<PlayerBike>();
            }
            if (state == State.Idle)
            {
                var pos = transform.position;
                pos.Y = baseY + 1.0f * MathF.Sin(Time.time * 2.0f);

                transform.position = pos;

                shadowRenderer.transform.position = transform.position.X0Z();

                if (player != null)
                {
                    float dist = Vector3.Distance(player.transform.position, transform.position);
                    if (dist < 10)
                    {
                        state = State.Attack;
                        attackTimer = attackCooldown;
                    }
                }

                targetObjectRenderer.enable = false;
            }
            else if (state == State.Attack)
            {
                // Base pos
                Vector3 targetPos = player.transform.position + player.transform.forward * 2.5f + Vector3.UnitY * 3.5f;
                // Losing ground
                targetPos -= player.transform.forward * distanceToTarget;
                // Just some animation
                targetPos += player.transform.right * 2.0f * MathF.Sin(Time.time * 1.5f);

                Vector3 currentPos = transform.position;

                transform.position = currentPos + (targetPos - currentPos) * 0.1f;
                
                // Targeting 
                if (attackTimer < 0.0f)
                {
                    if (lockTimer < lockTime)
                    {
                        targetObjectRenderer.enable = true;
                        targetObjectRenderer.color = Color4.Yellow;
                        lockPos = 1.0f * MathF.Sin(Time.time * 1.0f);
                        targetObjectRenderer.transform.position = player.transform.position + player.transform.right * lockPos + Vector3.UnitY * 0.2f;

                        lockTimer += Time.deltaTime;
                        shootTimer = shootCooldown;
                        currentShots = shotsPerAttack;
                    }
                    else
                    {
                        shootTimer -= Time.deltaTime;
                        if ((shootTimer <= 0) && (player.healthPercentage > 0))
                        {
                            shootTimer = shootCooldown;
                            currentShots--;
                            if (currentShots == 0)
                            {
                                attackTimer = attackCooldown;
                            }

                            GameObject explosionObject = new GameObject();
                            explosionObject.transform.position = targetObjectRenderer.transform.position;
                            explosionObject.transform.SetParent(player.transform);
                            var explosion = explosionObject.AddComponent<Explosion>();
                            explosion.scale = 2.0f;

                            float dist = Vector3.Distance(player.transform.position, targetObjectRenderer.transform.position);
                            if (dist < 0.5f)
                            {
                                player.DealDamage(20.0f);
                            }
                        }

                        targetObjectRenderer.color = Color4.Red;

                        var pos = MathHelpers.MoveTowards(targetObjectRenderer.transform.position, player.transform.position, 20.0f * Time.deltaTime);
                        pos.Y = 0.2f;
                        targetObjectRenderer.transform.position = pos;
                    }
                }
                else
                {
                    attackTimer -= Time.deltaTime;
                    lockTimer = 0.0f;
                    targetObjectRenderer.enable = false;
                }

                // Loose ground
                // Get bike speed
                if (player.speed < 0.5f)
                {
                    distanceToTarget -= 2.0f * Time.deltaTime;
                    if (distanceToTarget < 0) distanceToTarget = 0.0f;
                }
                else
                {
                    distanceToTarget += 1.0f * Time.deltaTime;
                    if (distanceToTarget > 5.0f)
                    {
                        // Teleport to start position
                        transform.position = startPosition;
                        state = State.Idle;
                    }
                }
            }
        }
    }
}
