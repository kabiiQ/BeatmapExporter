using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatmapExporterCore.Exporters
{
    /// <summary>
    /// Base class for BeatmapExporterCore exceptions
    /// </summary>
    [Serializable]
    public class ExporterException : Exception
    {
        public ExporterException() { }
        public ExporterException(string message) : base(message) { }
        public ExporterException(string message, Exception inner) : base(message, inner) { }
    }
}
