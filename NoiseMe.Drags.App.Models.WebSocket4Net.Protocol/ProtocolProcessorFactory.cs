using NoiseMe.Drags.App.DTO.Linq;

namespace NoiseMe.Drags.App.Models.WebSocket4Net.Protocol
{
	internal class ProtocolProcessorFactory
	{
		private IProtocolProcessor[] m_OrderedProcessors;

		public ProtocolProcessorFactory(params IProtocolProcessor[] processors)
		{
			m_OrderedProcessors = processors.OrderByDescending((IProtocolProcessor p) => (int)p.Version).ToArray();
		}

		public IProtocolProcessor GetProcessorByVersion(WebSocketVersion version)
		{
			return m_OrderedProcessors.FirstOrDefault((IProtocolProcessor p) => p.Version == version);
		}

		public IProtocolProcessor GetPreferedProcessorFromAvialable(int[] versions)
		{
			foreach (int item in versions.OrderByDescending((int i) => i))
			{
				IProtocolProcessor[] orderedProcessors = m_OrderedProcessors;
				foreach (IProtocolProcessor protocolProcessor in orderedProcessors)
				{
					int version = (int)protocolProcessor.Version;
					if (version < item)
					{
						break;
					}
					if (version <= item)
					{
						return protocolProcessor;
					}
				}
			}
			return null;
		}
	}
}
