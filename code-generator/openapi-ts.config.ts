// https://heyapi.dev/

import { defineConfig } from '@hey-api/openapi-ts';

const formatName = (str: string, isUpper = true) => {
  if (!str) return ''; // 如果字符串为空，则返回空字符串
  const firstLetter = str.charAt(0);
  return isUpper
    ? firstLetter.toUpperCase()
    : firstLetter.toLocaleLowerCase() + str.slice(1);
};

export default defineConfig({
  client: '@hey-api/client-axios',
  // input: 'https://localhost:44325/swagger/v1/swagger.json',
  input: 'http://10.0.5.20:8052/swagger/v1/swagger.json',
  output: {
    format: 'prettier',
    path: 'src/client',
    // lint: 'eslint'
    lint: false,
  },
  // plugins: [
  //   "@hey-api/schemas",
  //   "@hey-api/services",
  //   {
  //     serviceNameBuilder: "{{name}}Service",
  //     name: "@hey-api/services"
  //   },
  //   {
  //     dates: true,
  //     name: "@hey-api/transformers"
  //   },
  //   {
  //     enums: "typescript",
  //     name: "@hey-api/types"
  //   }
  // ]
  // services: {
  //   asClass: false,
  //   name: "{{name}}Service",
  //   methodNameBuilder(operation) {
  //     console.log("operation", operation);
  //     const parts = operation.name.split("Controller");
  //     // return formatName(parts[1], false);
  //     return parts.join("");
  //   }
  //   // operationId: true
  //   // response: 'response'
  // },
  // types: {
  //   enums: "typescript"
  // },
  // schemas: {
  //   type: "form"
  // }
});
