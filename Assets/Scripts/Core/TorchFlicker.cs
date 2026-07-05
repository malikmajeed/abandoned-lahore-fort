using UnityEngine;

namespace ForgottenFort.Core
{
    /// <summary>
    /// Simple torch flicker animation for atmosphere.
    /// </summary>
    public class TorchFlicker : MonoBehaviour
    {
        public Sprite[] frames;
        public float frameRate = 8f;

        SpriteRenderer sr;
        float timer;
        int frame;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (frames == null || frames.Length == 0)
            {
                frames = new Sprite[4];
                for (int i = 0; i < 4; i++)
                {
                    var tex = Resources.Load<Texture2D>($"Sprites/Objects/torch_{i}");
                    if (tex != null)
                        frames[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 32f);
                }
            }
        }

        void Update()
        {
            if (frames == null || frames.Length == 0 || sr == null) return;
            timer += Time.deltaTime;
            if (timer >= 1f / frameRate)
            {
                timer = 0;
                frame = (frame + 1) % frames.Length;
                if (frames[frame] != null) sr.sprite = frames[frame];
            }
        }
    }
}
