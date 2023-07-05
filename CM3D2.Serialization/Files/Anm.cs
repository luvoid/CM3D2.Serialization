
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using CM3D2.Serialization.Collections;

namespace CM3D2.Serialization.Files
{
	public class Anm : ICM3D2Serializable
	{
		public string signature = "CM3D2_ANIM";
		public int version;
		public List<Track> tracks = new List<Track>(); // Uses unique list serialization

		public class Track
		{
			public readonly byte channelId = 1; // Tracks always have a channel id of 1 (because they are tracks and not channels)
			public string path;
			public List<Channel> channels = new List<Channel>(); // Uses unique list serialization

			public class Channel
			{
				public byte channelId;
				public LengthPrefixedArray<Keyframe> keyframes = new LengthPrefixedArray<Keyframe>();

				[StructLayout(LayoutKind.Sequential, Pack = 1)]
				public struct Keyframe
				{
					float time;
					float value;
					float tanIn;
					float tanOut;
				}
			}
		}

		void ICM3D2Serializable.WriteWith(CM3D2Formatter formatter)
		{
			formatter.Write(signature);
			formatter.Write(version);
			foreach (var track in tracks)
			{
				formatter.Write(track.channelId);
				formatter.Write(track.path);
				foreach (var channel in track.channels)
				{
					formatter.Write(channel.channelId);
					channel.keyframes.WriteWith(formatter);
				}
			}
		}

		void ICM3D2Serializable.ReadWith(CM3D2Formatter formatter)
		{
			throw new NotImplementedException();
		}
	}
}
