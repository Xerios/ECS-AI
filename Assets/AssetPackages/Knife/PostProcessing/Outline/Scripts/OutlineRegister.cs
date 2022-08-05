using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Knife.PostProcessing
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Renderer))]
    public class OutlineRegister : MonoBehaviour
    {
        public Color OutlineTint = new Color(1, 1, 1, 1);
        private Renderer cachedRenderer;

        public Renderer CachedRenderer
        {
            get
            {
                if (cachedRenderer == null)
                    cachedRenderer = GetComponent<Renderer>();

                return cachedRenderer;
            }
        }

        void OnEnable()
        {
            OutlineRenderer.AddRenderer(CachedRenderer);
            setupPropertyBlock();
        }

        private void OnValidate()
        {
            setupPropertyBlock();
        }

        void setupPropertyBlock()
        {
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            CachedRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetColor("_OutlineColor", OutlineTint);
            CachedRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        void OnDisable()
        {
            OutlineRenderer.RemoveRenderer(CachedRenderer);
        }
    }
}