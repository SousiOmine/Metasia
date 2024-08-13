using Metasia.Core.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
    public class LayerObject : MetasiaObject, IMetaDrawable, IMetaAudiable
    {
        public List<MetasiaObject> Objects { get; protected set; } = new();
        public double Volume { get; set; }

        public LayerObject(string id) : base(id)
        {
        }

        public void AudioExpresser(ref AudioExpresserArgs e, int frame)
        {
            throw new NotImplementedException();
        }

        public void DrawExpresser(ref DrawExpresserArgs e, int frame)
        {
            throw new NotImplementedException();
        }
    }
}
