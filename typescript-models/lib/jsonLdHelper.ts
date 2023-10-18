import { EngineeringSymbolDto } from "./EngineeringSymbolModels";

export function jsonLdResponseToDto(response: object): EngineeringSymbolDto[] {
  if (!response) {
    throw new Error("jsonLdResponseToDto: response is null or undefined");
  }

  if ("@graph" in response && Array.isArray(response["@graph"])) {
    // Multiple symbols
    return response["@graph"].map(jsonLdSymbolToDto);
  } else if ("@id" in response) {
    // Single symbol
    return [jsonLdSymbolToDto(response)];
  }

  return [];
}

function jsonLdSymbolToDto(obj: object): EngineeringSymbolDto {
  const id = obj["@id"].split("/").pop();

  const result: EngineeringSymbolDto = {
    id: id,
    iri: obj["@id"],
    version: obj["pav:version"],
    previousVersion: null,
    previousVersionIri: null,
    dateTimeCreated: obj["dc:created"],
    dateTimeModified: obj["dc:modified"],
    dateTimeIssued: obj["dc:issued"],

    identifier: obj["dc:identifier"],
    label: obj["rdfs:label"],
    description: obj["dc:description"],
    sources: obj["dc:source"],
    subjects: obj["dc:subject"],
    creators: toArray(obj["dc:creator"], (o) => ({
      name: o["foaf:name"],
      email: o["foaf:mbox"],
    })),
    contributors: toArray(obj["dc:contributor"], (o) => ({
      name: o["foaf:name"],
      email: o["foaf:mbox"],
    })),
    shape: {
      serializations: (obj["sym:hasShape"]["sym:hasSerialization"] as []).map(
        (o) => ({ type: o["@type"], value: o["@value"] })
      ),
      depictions: obj["sym:hasShape"]["foaf:depiction"] as [],
    },
    width: parseInt(obj["sym:width"]),
    height: parseInt(obj["sym:height"]),
    drawColor: obj["sym:drawColor"],
    fillColor: obj["sym:fillColor"],
    centerOfRotation: {
      x: parseFloat(obj["sym:hasCenterOfRotation"]["sym:positionX"]),
      y: parseFloat(obj["sym:hasCenterOfRotation"]["sym:positionY"]),
    },
    connectionPoints: toArray(obj["sym:hasConnectionPoint"], (o) => ({
      identifier: o["dc:identifier"],
      position: {
        x: parseFloat(o["sym:positionX"]),
        y: parseFloat(o["sym:positionY"]),
      },
      direction: parseFloat(o["sym:connectorDirection"]),
    })),
  };

  if (obj["esmde:status"]) {
    result.status = obj["esmde:status"];
  }

  if (obj["esmde:oid"]) {
    result.userIdentifier = obj["esmde:oid"];
  }

  if (
    obj["pav:previousVersion"] &&
    "@id" in obj["pav:previousVersion"] &&
    obj["pav:previousVersion"]["@id"]
  ) {
    const iri = obj["pav:previousVersion"]["@id"];
    result.previousVersion = iri.split("/").pop();
    result.previousVersionIri = iri;
  }

  return result;
}

function toArray<T>(obj: object, fn: (o: object) => T): T[] {
  return Array.isArray(obj) ? obj.map(fn) : [fn(obj)];
}
