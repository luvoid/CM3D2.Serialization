
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using CM3D2.Serialization.Collections;

namespace CM3D2.Serialization.Files
{
	public class Anm : ICM3D2Serializable, ISummarizable
	{
		public readonly string signature = "CM3D2_ANIM";
		public int version = 1000;
		public List<Track> tracks = new List<Track>(); // Uses unique list serialization

		public class Track
		{
			/// <summary>
			/// Tracks always have a channel id of 1 (because they are tracks and not channels)
			/// </summary>
			public readonly byte channelId = 1;
			public string path;
			public List<Channel> channels = new List<Channel>(); // Uses unique list serialization

			public class Channel
			{
				/// <summary>
				/// Must be greater than 1
				/// </summary>
				public byte channelId = 2;
				public LengthPrefixedArray<Keyframe> keyframes = new LengthPrefixedArray<Keyframe>(0);

				[StructLayout(LayoutKind.Sequential, Pack = 1)]
				public struct Keyframe
				{
					public float time;
					public float value;
					public float tanIn;
					public float tanOut;
				}
			}
		}

		/// <summary>
		/// If <see cref="useMuneKey"/> has no value, then the bytes are not written or read.
		/// </summary>
		public MuneKeyUsage? useMuneKey;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct MuneKeyUsage
		{
			[MarshalAs(UnmanagedType.U1)]
			public bool left;

			[MarshalAs(UnmanagedType.U1)]
			public bool right;
		}


		void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
		{
			writer.Write(signature);
			writer.Write(version);
			foreach (var track in tracks)
			{
				writer.Write(track.channelId);
				writer.Write(track.path);
				foreach (var channel in track.channels)
				{
					if (channel.channelId <= 1)
					{
						throw new ArgumentOutOfRangeException(nameof(channel.channelId), "Channel.channelId must be a value greater than 1");
					}
					writer.Write(channel.channelId);
					writer.Write(channel.keyframes);
				}
			}

			writer.Write((byte)0);
			writer.Write(useMuneKey);
		}

		void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
		{
			reader.Read(out string readSignature);
			if (readSignature != signature) throw new FormatException($"Expected signature \"{signature}\" but instead found \"{readSignature}\"");
			
			reader.Read(out version);

			while (reader.Peek<byte>() == 1)
			{
				Track track = new Track();
				tracks.Add(track);
				reader.Read(out byte _); // track.channelId
				reader.Read(out track.path);

				while (reader.Peek<byte>() > 1)
				{
					Track.Channel channel = new Track.Channel();
					track.channels.Add(channel);
					reader.Read(out channel.channelId);
					reader.Read(out channel.keyframes);
				}
			}

			reader.Read(out byte endByte);
			if (endByte != 0)
			{
				throw new FormatException($"Unexpected channelId {endByte} (expected 0)");
			}

			reader.Read(out useMuneKey);
		}


		public string Summarize()
		{
			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.Append($"Anm {{ {signature} v{version} Track[{tracks.Count}]");
			if (tracks.Count > 0)
			{
				stringBuilder.Append($" {{Channel[{tracks[0].channels.Count}]");
				if (tracks[0].channels.Count > 0)
				{
					stringBuilder.Append($"{{Keyframe[{tracks[0].channels[0].keyframes.Length}]");

					if (tracks[0].channels.Count > 1) stringBuilder.Append(",...");
					stringBuilder.Append("}");
				}

				if (tracks.Count > 1) stringBuilder.Append(", ...");
				stringBuilder.Append("}");
			}
			stringBuilder.Append(" }");

			return stringBuilder.ToString();
		}
	}
}
