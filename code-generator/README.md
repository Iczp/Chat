# Net9 代码生成器

## 代码生成器

1. 初始化项目

   ```bash
   mkdir code-gen
   cd code-gen
   npm init -y
   ```

2. 安装 Plop

   ```bash
   pnpm install --save-dev plop
   ```

3. 创建 Plopfile.js

```bash
plop --init-ts
```

4. install [tsx](https://github.com/privatenumber/tsx) and optionally [cross-env](https://www.npmjs.com/package/cross-env):

```bash
npm i -D tsx cross-env
```

Finally, use `NODE_OPTIONS` to activate the tsx loader. Now Plop can import your `plopfile.ts`:

**Node.js v20.6 and above**

```
// package.json
"scripts": {
  "cross-env NODE_OPTIONS='--import tsx' plop --plopfile=plopfile.ts"
}
```

**Node.js v20.5.1 and below**

```
// package.json
"scripts": {
  "cross-env NODE_OPTIONS='--loader tsx' plop --plopfile=plopfile.ts"
}
```

## Openapi-ts 生成 client (swagger 文档)

1. 安装 openapi-ts

   ```bash
   pnpm install --save-dev openapi-ts
   ```

2. 创建 openapi-ts 配置文件

   文档地址： https://heyapi.dev/

   ```ts
   // https://heyapi.dev/
   import { defineConfig } from '@hey-api/openapi-ts';

   export default defineConfig({
     client: '@hey-api/client-axios',
     input: 'http://localhost:3000/swagger.json',
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
   ```

3. 安装依赖 @hey-api/client-axios

```bash
pnpm install @hey-api/client-axios
```

## Usage 使用

### CURD 生成器

```
npx plop order
```

Clent 生成

```bash
pnpm openapi-ts
```
