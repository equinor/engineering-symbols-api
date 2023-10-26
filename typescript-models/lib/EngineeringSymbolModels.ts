export type EngineeringSymbolDto = {
  id: string;
  iri: string;
  status?: string;
  version: string;
  previousVersion: string | null;
  previousVersionIri: string | null;
  dateTimeCreated: string;
  dateTimeModified: string;
  dateTimeIssued: string;
} & Omit<EngineeringSymbolPutDto, "isRevisionOf">;

/** Model for CREATING and UPDATING a symbol */
export type EngineeringSymbolPutDto = {
  identifier: string;
  label: string;
  description: string;
  sources: string[] | null;
  subjects: string[] | null;
  userIdentifier?: string;
  creators: {
    name: string;
    email: string;
  }[];
  contributors: {
    name: string;
    email: string;
  }[];
  shape: {
    serializations: {
      type: string;
      value: string;
    }[];
    depictions?: string[];
  };
  width: number;
  height: number;
  drawColor?: string;
  fillColor?: string;
  centerOfRotation: {
    x: number;
    y: number;
  };
  connectionPoints: ConnectionPoint[];
};

export type ConnectionPoint = {
  identifier: string;
  position: {
    x: number;
    y: number;
  };
  direction: number;
};
