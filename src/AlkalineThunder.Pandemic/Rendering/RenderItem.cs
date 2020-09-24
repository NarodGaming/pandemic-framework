using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkalineThunder.Pandemic.Rendering
{
    internal class RenderItem
    {
        private int _vertexPointer;
        private int _indexPointer;
        private VertexPositionColorTexture[] _vbo;
        private int[] _ibo;
        private float _renderOpacity;
        private Transform _transform;
        
        public int Triangles => _indexPointer / 3;
        
        public Texture2D Texture { get; set; }

        public VertexPositionColorTexture[] Vertices => _vbo;
        public int[] IndexBuffer => _ibo;

        public RenderItem(float renderOpacity, Transform transform)
        {
            _transform = transform;
            _renderOpacity = renderOpacity;
            _vbo = new VertexPositionColorTexture[128];
            _ibo = new int[128];
        }

        public void AddIndex(int index)
        {
            _ibo[_indexPointer] = index;
            _indexPointer++;
            if (_indexPointer >= _ibo.Length)
                Array.Resize(ref _ibo, _ibo.Length * 2);
        }
        
        public int AddVertex(Vector2 position, Color color, Vector2 texCoord)
        {
            var i = _vertexPointer;

            if (i >= _vbo.Length)
            {
                Array.Resize(ref _vbo, _vbo.Length * 2);
            }

            var transformed = _transform.PerformTransform(position);
            var v = new VertexPositionColorTexture(new Vector3(transformed.X, transformed.Y, 0), color * _renderOpacity, texCoord);
            _vbo[_vertexPointer] = v;
            _vertexPointer++;
            
            return i;
        }

    }
}
