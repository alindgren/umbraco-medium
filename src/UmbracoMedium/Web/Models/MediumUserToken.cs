using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

/// <summary>
/// MediumUserToken is used to store
/// </summary>
[TableName("MediumUserToken")]
[PrimaryKey("Id", autoIncrement = true)]
public class MediumUserToken
{
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

}