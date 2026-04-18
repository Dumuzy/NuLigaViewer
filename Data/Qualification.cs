namespace NuLigaViewer.Data
{
    public enum Qualification
    {
        None,
        Aufstieg,
        Abstieg
    }

    public static class QualificationHelper
    {
        public static Qualification ParseQualification(string qualificationStr)
        {
            return qualificationStr switch
            {
                "Aufsteiger" => Qualification.Aufstieg,
                "Absteiger" => Qualification.Abstieg,
                _ => Qualification.None
            };
        }
    }
}