using Unity.Services.Economy.Internal.Http;

public struct ItemAndAmountData
{
    public const string INVENTORY_ITEM = "INVENTORY_ITEM";

    public string id;
    public string type;
    public int amount;
    public IDeserializable customData;

    public ItemAndAmountData(string id, string type, int amount, IDeserializable customData)
    {
        this.id = id;
        this.type = type;
        this.amount = amount;
        this.customData = customData;
    }

    public override string ToString()
    {
        return $"{id}:{amount}";
    }
}
