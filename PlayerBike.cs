using OpenTK.Mathematics;

namespace OpenTKBase
{
    public class PlayerBike : Component
    {
        public override void Start()
        {
            GameObject playerShadow = new GameObject();
            playerShadow.transform.position = transform.position + Vector3.UnitY * 0.1f;
            playerShadow.transform.SetParent(transform);
            var sr = playerShadow.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.FindSprite("bike_shadow");

        }
    }
}
