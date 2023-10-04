using CM3D2.Serialization.Files;



class Class
{
	/// <summary> XML Comment Summary </summary>
	/// <remarks> XML Comment Remarks </remarks>
	[FileVersionConstraint(1000)]
	public bool Field0;


	/// <remarks> XML Comment Remarks </remarks>
	[FileVersionConstraint(2000)]
	public bool Field1;


	[FileVersionConstraint(FileVersions.COM3D2_1)]
	public bool Field2;
}