using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace CM3D2.Serialization.Files
{
	public class Menu : ICM3D2Serializable, ISummarizable
	{
		public readonly string signature = "CM3D2_MENU";
		public int version;
		public string srcFileName;
		public string itemName;
		public string category;
		public string infoText;

		/// <summary>The size of the remaining data in bytes.</summary>
		/// <remarks>The game will still load a .menu file in many cases even if this value is incorrect.</remarks>
		public int bodySize;

		[DeepSerialized]
		public List<Command> commands = new();

		[DeepSerializable]
		public class Command
		{
			public byte argCount;

			/// <summary>
			/// The first argument is the name of / an alias of the command.
			/// Must have at least 1 argument.
			/// </summary>
			[LengthDefinedBy(nameof(argCount))]
			public LengthDefinedStringList args;

			public Command()
			{
				this.args = new();
			}

			public Command(IEnumerable<string> args)
			{
				this.args = new(args);
				argCount = (byte)this.args.Count;
			}

			public Command(params string[] args)
			{
				this.args = new(args);
				argCount = (byte)args.Length;
			}
		}

		public readonly byte endByte = 0;


		/// <summary>
		/// Update the value of <see cref="bodySize"/> based on the current content.
		/// The <see cref="bodySize"/> is always updated right before serializing.
		/// </summary>
		public void UpdateBodySize()
		{
			int sum = 0;

			foreach (var command in commands)
			{
				sum += sizeof(byte); // argCount
				foreach (var arg in command.args)
				{
					int encodedStringLength = Encoding.UTF8.GetByteCount(arg);
					sum += LEB128.SizeOfValue(encodedStringLength) + encodedStringLength;
				}
			}

			sum += sizeof(byte); // endByte

			bodySize = sum;
		}


		void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
		{
			reader.Read(out string temp_signature);
			if (temp_signature != signature)
				throw new FormatException($"Expected {nameof(signature)} \"{signature}\" but instead found \"{temp_signature}\"");
			reader.Read(out version);
			reader.Read(out srcFileName);
			reader.Read(out itemName);
			reader.Read(out category);
			reader.Read(out infoText);
			reader.Read(out bodySize);

			while (reader.PeekByte() != 0)
			{
				Command command = new();
				commands.Add(command);

				reader.Read(out command.argCount);
				
				command.args.SetLength(command.argCount);
				reader.Read(ref command.args);

			}

			reader.Read(out byte temp_endByte);
			if (temp_endByte != endByte)
				throw new FormatException($"Expected {nameof(endByte)} \"{endByte}\" but instead found \"{temp_endByte}\"");
		}

		void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
		{
			writer.Write(signature);
			writer.Write(version);
			writer.Write(srcFileName);
			writer.Write(itemName);
			writer.Write(category);
			writer.Write(infoText);

			UpdateBodySize();
			writer.Write(bodySize);

			foreach (var command in commands)
			{
				if (command.args.Count > byte.MaxValue)
				{
					throw new FormatException($"The command \"{string.Join(" ", command.args.ToArray())}\" has too many arguments ({command.args.Count}). Max is 255.");
				}
				else if (command.args.Count == 0)
				{
					throw new FormatException($"Commands with 0 arguments are not allowed.");
				}
				command.argCount = (byte)command.args.Count;
				writer.Write(command.argCount);

				command.args.ValidateLength(command.argCount);
				writer.Write(command.args);
			}

			writer.Write(endByte);
		}

		public string Summarize()
		{
			return $"{{ {signature} v{version} \"{itemName}\" commands[{commands.Count}] (bodySize: {bodySize}) }}";
		}
	}
}
