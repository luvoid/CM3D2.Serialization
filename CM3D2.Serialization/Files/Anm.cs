
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
	public class Anm : ICM3D2Serializable, ISummarizable
	{
		public readonly string signature = "CM3D2_ANIM";
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

		void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
		{
			writer.Write(signature, Encoding.ASCII);
			writer.Write(version);
			foreach (var track in tracks)
			{
				writer.Write(track.channelId);
				writer.Write(track.path);
				foreach (var channel in track.channels)
				{
					writer.Write(channel.channelId);
					writer.Write(channel.keyframes);
				}
			}
			writer.Write((byte)0);
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
				reader.Read<byte>(out _); // track.channelId
				reader.Read(out track.path);

				while (reader.Peek<byte>() > 1)
				{
					reader.DebugLogStreamPosition("channel");
					Track.Channel channel = new Track.Channel();
					track.channels.Add(channel);
					reader.Read(out channel.channelId);
					reader.Read(out channel.keyframes);
				}
			}

			reader.DebugLogStreamPosition("final byte");
			reader.Read(out byte finalByte);
			if (finalByte != 0)
			{
				throw new FormatException($"Unexpected channelId {finalByte} (expected 0)");
			}
		}


		public string Summarize()
		{
			return 
				$"Anm {{ " +
					$"{signature} v{version} Track[{tracks.Count}] {{" +
						$"Channel[{tracks[0].channels.Count}] {{" +
							$"Keyframe[{tracks[0].channels[0].keyframes.Length}], " +
							$"..." +
						$"}}, " +
						$"..." +
					$"}}" +
				$" }}";
		}
	}
}
