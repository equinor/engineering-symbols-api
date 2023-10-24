namespace EngineeringSymbols.Tools.Constants;

public static class EsProp
{
    public const string IsType = "type";
    public const string IsTypeIri = $"{Ontology.RdfIri}{IsType}";
    public const string IsTypeQName = $"rdf:{IsType}";
    
    public const string IdQname = $"{Ontology.MetadataEditorPrefix}:id";
    
    public const string SymbolTypeQname = $"{Ontology.EngSymPrefix}:{Ontology.SymbolTypeName}";
    
    public const string Label = "label";
    public const string LabelIri = $"{Ontology.RdfsIri}{Label}";
    public const string LabelQName = $"rdfs:{Label}";
    
    public const string HasEngSymId = "id";
    public const string HasEngSymIdIri = $"{Ontology.EngSymOntologyIri}{HasEngSymId}";
    public const string HasEngSymIdQName = $"{Ontology.EngSymPrefix}:{HasEngSymId}";
    
    public const string Identifier = "identifier";
    public const string IdentifierIri = $"{Ontology.DcIri}{Identifier}";
    public const string IdentifierQName = $"dc:{Identifier}";
    
    public const string EditorStatus = "status";
    public const string EditorStatusIri = $"{Ontology.MetadataEditorIri}{EditorStatus}";
    public const string EditorStatusQName = $"{Ontology.MetadataEditorPrefix}:{EditorStatus}";
    
    public const string UserObjectIdQName = $"{Ontology.MetadataEditorPrefix}:oid";
    
    public const string Description = "description";
    public const string DescriptionIri = $"{Ontology.DcIri}{Description}";
    public const string DescriptionQName = $"dc:{Description}";
    
    public const string SourceQName = "dc:source";
    public const string SubjectQName = "dc:subject";
    public const string VersionQName = "pav:version";
    public const string PreviousVersionQName = "pav:previousVersion";
    
    public const string HasShape = "hasShape";
    public const string HasShapeIri = $"{Ontology.EngSymOntologyIri}{HasShape}";
    public const string HasShapeQName = $"{Ontology.EngSymPrefix}:{HasShape}";

    public const string HasSerializationQName = $"{Ontology.EngSymPrefix}:hasSerialization";
    
    public const string Width = "width";
    public const string WidthIri = $"{Ontology.EngSymOntologyIri}{Width}";
    public const string WidthQName = $"{Ontology.EngSymPrefix}:{Width}";
    
    public const string Height = "height";
    public const string HeightIri = $"{Ontology.EngSymOntologyIri}{Height}";
    public const string HeightQName = $"{Ontology.EngSymPrefix}:{Height}";
    
    public const string Creator = "creator";
    public const string CreatorIri = $"{Ontology.DcIri}{Creator}";
    public const string CreatorQName = $"dc:{Creator}";
    
    public const string ContributorQName = $"dc:contributor";
    
    public const string HasConnectionPoint = "hasConnectionPoint";
    public const string HasConnectionPointIri = $"{Ontology.EngSymOntologyIri}{HasConnectionPoint}";
    public const string HasConnectionPointQName = $"{Ontology.EngSymPrefix}:{HasConnectionPoint}";
    
    public const string DateCreatedQName = "dc:created";
    public const string DateModifiedQName = "dc:modified";
    public const string DateIssuedQName = "dc:issued";
    
    public const string PositionXQName = $"{Ontology.EngSymPrefix}:positionX";
    public const string PositionYQName = $"{Ontology.EngSymPrefix}:positionY";
    
    public const string DrawColorQName = $"{Ontology.EngSymPrefix}:drawColor";
    public const string FillColorQName = $"{Ontology.EngSymPrefix}:fillColor";    
        
    public const string HasCenterOfRotationQName = $"{Ontology.EngSymPrefix}:hasCenterOfRotation";
    
    public const string ConnectorDirectionQName = $"{Ontology.EngSymPrefix}:connectorDirection";
}