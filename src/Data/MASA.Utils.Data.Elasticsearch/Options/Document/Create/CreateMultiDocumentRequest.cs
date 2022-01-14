namespace MASA.Utils.Data.Elasticsearch.Options.Document.Create;

public class CreateMultiDocumentRequest<TDocument> : DocumentOptions where TDocument : class
{
    public List<SingleDocumentBaseRequest<TDocument>> Items { get; set; }

    public CreateMultiDocumentRequest(string indexName) : base(indexName)
    {
        Items = new();
    }

    public CreateMultiDocumentRequest(string indexName, TDocument document, string? documentId = null) : this(indexName)
    {
        this.AddDocument(document, documentId);
    }

    public CreateMultiDocumentRequest<TDocument> AddDocument(TDocument document, string? documentId = null)
    {
        return this.AddDocument(new SingleDocumentBaseRequest<TDocument>(document, documentId));
    }

    public CreateMultiDocumentRequest<TDocument> AddDocument(SingleDocumentBaseRequest<TDocument> item)
    {
        Items.Add(item);
        return this;
    }
}
