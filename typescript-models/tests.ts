import singleResponse from "./responses/GET_single_graph.json";
import multipleResponse from "./responses/GET_multiple_graphs.json";
import { jsonLdResponseToDto } from "./lib/jsonLdHelper";

console.log("Single json ld response:");

var result1 = jsonLdResponseToDto(singleResponse);

console.log(JSON.stringify(result1, null, 2));

console.log("Multiple json ld response:");

var result2 = jsonLdResponseToDto(multipleResponse);

console.log(JSON.stringify(result2, null, 2));
