using BenchmarkDotNet.Running;
using Guider;

BenchmarkRunner.Run<Benchmarks>();
return;

var id = Guid.Parse("83050558-771a-4ae3-a516-85a73e0c0b98");
Console.WriteLine(id);

var base64Id = Convert.ToBase64String(id.ToByteArray());
Console.WriteLine(base64Id);

var urlFriendlyBase64Id1 = GuiderBase.ToStringFromGuid(id);
var urlFriendlyBase64Id2 = GuiderNick.ToStringFromGuid(id);
var urlFriendlyBase64Id3 = GuiderMine_1.ToStringFromGuid(id);
var urlFriendlyBase64Id4 = GuiderMine_2.ToStringFromGuid(id);
var urlFriendlyBase64Id5 = GuiderMine_3.ToStringFromGuid(id);
var urlFriendlyBase64Id6 = GuiderMine_4.ToStringFromGuid(id);
Console.WriteLine(urlFriendlyBase64Id1);
Console.WriteLine(urlFriendlyBase64Id2);
Console.WriteLine(urlFriendlyBase64Id3);
Console.WriteLine(urlFriendlyBase64Id4);
Console.WriteLine(urlFriendlyBase64Id5);
Console.WriteLine(urlFriendlyBase64Id6);

var idAgain1 = GuiderBase.ToGuidFromString("WAUFgxp340qlFoWnPgwLmA");
var idAgain2 = GuiderNick.ToGuidFromString("WAUFgxp340qlFoWnPgwLmA");
var idAgain3 = GuiderMine_1.ToGuidFromString("WAUFgxp340qlFoWnPgwLmA");
var idAgain4 = GuiderMine_2.ToGuidFromString("WAUFgxp340qlFoWnPgwLmA");
var idAgain5 = GuiderMine_3.ToGuidFromString("WAUFgxp340qlFoWnPgwLmA");
var idAgain6 = GuiderMine_4.ToGuidFromString("WAUFgxp340qlFoWnPgwLmA");
Console.WriteLine(idAgain1);
Console.WriteLine(idAgain2);
Console.WriteLine(idAgain3);
Console.WriteLine(idAgain4);
Console.WriteLine(idAgain5);
Console.WriteLine(idAgain6);