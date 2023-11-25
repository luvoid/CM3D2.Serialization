# CM3D2.Serialization

The serializer to end all serializers.



## Defining Types

Instead of using a large and complicated import and export function,
CM3D2.Serialization can instead use a class/struct that defines fields 
in the same order as they are stored in the file.
This way an entire file can simply be deserialized as a single instance of a class.

For example, if the binary of a .mate file looks like this:
```
Field  | signature                                    version     name           ...
Type   | string                                       Int32       string         ...
Value  |    C  M  3  D  2  _  M  A  T  E  R  I  A  L  1000           h  a  i  r  ...
Binary | 0E 43 4D 33 44 32 5F 4D 41 54 45 52 49 41 4C E8 03 00 00 13 68 61 69 72 ...
```
A simple class definition with fields that match the layout of the file's binary
is enough to tell the `CM3D2Serializer` how to serialize the type:
```cs
[AutoCM3D2Serializable]
public partial class Mate : ICM3D2Serializable
{
    public string signature;
    public int version;
    public string name;
    // ...
}
```
And a serializer can now read a .mate file from a filestream as a `Mate` class:
```cs
var serializer = new CM3D2Serializer();
var mate = serializer.Deserialize<Mate>(filestream);
Console.WriteLine(mate.name);
Console.WriteLine(mate.version);
// Console Output:
// hair...
// 1000
```

More complicated types can have further control by manually implementing
`ICM3D2Serializable.ReadWith(ICM3D2Reader reader)` and `ICM3D2Serializable.WriteWith(ICM3D2Writer writer)`

## Gotchas

- Q: Is reflection being used to find the serializable fields in a type marked with the `[AutoCM3D2Serializable]` attribute?
Doesn't reflection have large performance drawbacks?
- A: No, the use of reflection is very minimal in this project.
Instead, two functions are generated at compile time that serialize / deserialize the type on-demand.

* Q: How are structs that don't implement `ICM3D2Serializable` serialized?
* A: In the case of an unmanaged struct *S*, the amount of memory allocated to hold it's data on the heap is a fixed number.
An instance of struct *S* can be quickly deserialized by reading in a byte array of the same length,
and tricking the .NET runtime into beliving that byte array is instead an allocation of struct *S* on the heap.
