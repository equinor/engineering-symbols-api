# Symbol Editor
This document describes in its entirety the "Symbol Editor" stack. It consists of several different components, which will be described below.


- [Symbol Editor](#symbol-editor)
  - [Fuseki](#fuseki)
  - [Symbol API](#symbol-api)
    - [Key dependencies](#key-dependencies)
  - [Engineering Symbols Library](#engineering-symbols-library)
  - [Symbol Editor Component](#symbol-editor-component)

The intent is to give an overview of how the stack as a whole functions, not a deep dive into each specific part.

---

## [Fuseki](https://jena.apache.org/documentation/fuseki2/)
A [Fuseki triple store](https://jena.apache.org/documentation/fuseki2/) instance is deployed in a container. The container, images and configurations are managed in an Azure Container Registry.

In order to limit access to the Fuseki instance the database is placed in a virtual network on Azure, which is closed to outside connections.

The API acts as a bridge and is the only service with access to the Fuseki instance.

## [Symbol API](https://github.com/equinor/engineering-symbols-api)
A dotnet HTTP API. Manages interaction between [Engineering Symbols Library](#engineering-symbols-library) and the Fuseki triple store.

Code is versioned and maintained in the Equinor Github, at [Engineering symbols API](https://github.com/equinor/engineering-symbols-api). The source code is made freely available as open source under the  [MIT License](https://github.com/equinor/engineering-symbols-api/blob/main/LICENSE)

When a `Symbol` is `Issued`, the API is the originator for the HTTP Post Request that attempts to send the newly minted `Symbol` to `Common Library`. When a `Symbol` is sent to Common Library, it is serialized as JSON-LD RDF and [framed](https://www.w3.org/TR/json-ld11-framing/) according to the framing found at the [Symbol-Ontology repository](https://github.com/equinor/symbol-vocabulary/blob/master/schema/symbol-context.json).  

The [Engineering Symbols Library](#engineering-symbols-library) has an internal working model of a `Symbol` which is serialized using JSON. The API manages the two way conversion between RDF and this JSON Serialization.

The JSON serialization is only intended for internal use and should not be communicated to any other service or actor than the [Engineering Symbols Library](#engineering-symbols-library).

All outwards facing endpoints, public and requiring authorization return framed JSON-LD.

### Key dependencies
- [dotnNetRDF](https://dotnetrdf.org/) v.3.0.0
  - Repository: [github.com/dotnetrdf/dotnetrdf](https://github.com/dotnetrdf/dotnetrdf)
- [Xunit](https://xunit.net/) v2.4.2
- [Swashbuckle.AspNetCore](https://www.nuget.org/packages/Swashbuckle.AspNetCore)
- [Serilog.AspNetCore](https://www.nuget.org/packages/Serilog.AspNetCore/)

## [Engineering Symbols Library](https://engineering-symbols.equinor.com/)

This is a reference implementation of a library of machine readable symbols for use in visual digital representations of structured data models, drafting functionality for said symbols and publishing to external repositories intended for perpetual storage and distribution of reference data.

The reference implementation has been built as a webpage using typescript and is currently deployed using [Radix](https://www.radix.equinor.com/).

The current deployed instance of the Engineering Symbol Library and the collection of symbols hosted is made publicly available at [Engineering symbols](https://engineering-symbols.equinor.com/). The source code itself is also made freely available under the [MIT License](https://github.com/equinor/engineering-symbols/blob/master/LICENSE), on [Engineering symbols](https://github.com/equinor/engineering-symbols).

## Symbol Editor Component