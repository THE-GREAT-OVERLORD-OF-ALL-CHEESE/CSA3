namespace CheeseMods.CSA3
{
    public class ObjectReplacement
    {
        public string originalId;
        public string newId;
        public bool active;

        public ObjectReplacement(string originalId, string newId)
        {
            this.originalId = originalId;
            this.newId = newId;
            active = true;
        }
    }
}
