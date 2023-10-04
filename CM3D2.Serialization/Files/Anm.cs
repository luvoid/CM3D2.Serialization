
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Types;

namespace CM3D2.Serialization.Files
{
    public class Anm : ICM3D2Serializable, ISummarizable
	{
		public enum ChannelIdType : byte
		{
			None            =   0,
			Track           =   1,

			LocalRotationX  = 100,
			LocalRotationY  = 101,
			LocalRotationZ  = 102,
			LocalRotationW  = 103,
			LocalPositionX  = 104,
			LocalPositionY  = 105,
			LocalPositionZ  = 106,
			MainTexX        = 107,
			MainTexY        = 108,
			MainTexZ        = 109,
			MainTexW        = 110,
			OutlineTexX     = 111,
			OutlineTexY     = 112,
			OutlineTexZ     = 113,
			OutlineTexW     = 114,
			ShadowTexX      = 115,
			ShadowTexY      = 116,
			ShadowTexZ      = 117,
			ShadowTexW      = 118,
			UVOffsetX       = 119,
			UVOffsetY       = 120,

			ExLocalScaleX   = 200,
			ExLocalScaleY   = 201,
			ExLocalScaleZ   = 202,
			ExBlendValue    = 203
		}

		public readonly string signature = "CM3D2_ANIM";
		public int version = (int)FileVersions.CM3D2;
		public List<Track> tracks = new(); // Uses unique list serialization

		public class Track
		{
			/// <summary>
			/// Tracks always have a channel id of 1 (because they are tracks and not channels)
			/// </summary>
			public readonly ChannelIdType channelId = ChannelIdType.Track;
			public string path;
			public List<Channel> channels = new(); // Uses unique list serialization
		}

		public class Channel
		{
			/// <summary>
			/// Must be greater than 1
			/// </summary>
			public ChannelIdType channelId = ChannelIdType.LocalRotationX;
			public LengthPrefixedArray<Keyframe> keyframes = new();

		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Keyframe
		{
			public float time;
			public float value;
			public float inTangent;
			public float outTangent;
		}

		public Omittable<MuneKeyUsage> useMuneKey;

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
					if (channel.channelId <= ChannelIdType.Track)
					{
						throw new ArgumentOutOfRangeException(nameof(channel.channelId), "Channel.channelId must be a value greater than 1");
					}
					writer.Write(channel.channelId);
					writer.Write(channel.keyframes);
				}
			}

			writer.Write(ChannelIdType.None);
			writer.Write(useMuneKey);
		}

		void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
		{
			reader.Read(out string foundSignature);
			if (foundSignature != signature) throw new FormatException($"Expected signature \"{signature}\" but instead found \"{foundSignature}\"");
			
			reader.Read(out version);

			while (reader.Peek<ChannelIdType>() == ChannelIdType.Track)
			{
				Track track = new Track();
				tracks.Add(track);
				reader.Read(out byte _); // track.channelId
				reader.Read(out track.path);

				while (reader.Peek<ChannelIdType>() > ChannelIdType.Track)
				{
					Channel channel = new Channel();
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

			stringBuilder.Append($"{{ {signature} v{version} Track[{tracks.Count}]");
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
