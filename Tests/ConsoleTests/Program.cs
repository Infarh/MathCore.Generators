
using System.Windows.Input;

using MathCore.Generators.CSV;

//var p = new Person();

//var str = @"123,456,123,345";

//foreach (var s in str.AsStringPtr().Split(','))
//{
//    s.TryParseAsInt32(out var q);
//    var q = s.ParseDouble()
//}



//"123qwe123".IndexOf("QWE", StringComparison.OrdinalIgnoreCase)

Console.WriteLine("End.");
Console.ReadLine();

[CSVClass]
public partial class Person : IEquatable<Person>
{
    ///<summary>Идентификатор</summary>
    [CSVColumn]
    public int Id { get; set; }

    ///<summary>Имя</summary>
    [CSVColumn]
    public string Name { get; set; }

    ///<summary>Фамилия</summary>
    [CSVColumn]
    public string Surname { get; set; }

    ///<summary>Отчество</summary>
    [CSVColumn]
    public string Patronymic { get; set; }

    ///<summary>Дата рождения</summary>
    [CSVColumn]
    public DateTime Birthday { get; set; }

    #region IEquatable<Entity>

    public bool Equals(Person other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id
            && Name == other.Name
            && Surname == other.Surname
            && Patronymic == other.Patronymic
            && Birthday.Equals(other.Birthday);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Person)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash_code = EqualityComparer<int>.Default.GetHashCode(Id);

            var string_comparer = EqualityComparer<string>.Default;

            // ReSharper disable NonReadonlyMemberInGetHashCode
            hash_code = (hash_code * 397) ^ string_comparer.GetHashCode(Name);
            hash_code = (hash_code * 397) ^ string_comparer.GetHashCode(Surname);
            hash_code = (hash_code * 397) ^ string_comparer.GetHashCode(Patronymic);
            hash_code = (hash_code * 397) ^ EqualityComparer<DateTime>.Default.GetHashCode(Birthday);
            // ReSharper restore NonReadonlyMemberInGetHashCode

            return hash_code;
        }
    }

    public static bool operator ==(Person left, Person right) => Equals(left, right);
    public static bool operator !=(Person left, Person right) => !Equals(left, right);

    #endregion
}