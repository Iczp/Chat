// Plopfile.js
// https://plopjs.com/documentation/#getting-started
const { execSync } = require('child_process');
const pluralize = require('pluralize');
const fs = require('fs');
const path = require('path');

const project = `Chat`;
const namespace = `IczpNet.${project}`;
const srcPrefix = `../src/${namespace}`;
const localizationDir = `${srcPrefix}.Domain.Shared/Localization/${project}/`;

// 读取指定目录下的所有 .json 文件
function getJsonFilesInDirectorySync(dirPath) {
  console.log(`getJsonFilesInDirectorySync: `, dirPath);
  try {
    const files = fs.readdirSync(dirPath);
    const jsonFiles = files.filter((file) => path.extname(file) === '.json');
    return jsonFiles;
  } catch (err) {
    console.error(`Unable to read directory: ${err}`);
    throw err; // 重新抛出错误，让调用者处理
  }
}

module.exports = function (plop) {
  // Helper function to execute shell commands
  const exec = (command) => {
    try {
      execSync(command, { stdio: 'inherit' });
    } catch (error) {
      console.error(`Error executing command: ${command}`);
      throw error;
    }
  };

  const adds = [
    'Repository',
    // 'IRepository',
    'Manager',
    // 'IManager',
    'AppService',
    // 'IAppService',
    'GetListInput',
    'Dto',
    'CreateInput',
    'UpdateInput',
    'SampleDto',
    'DetailDto',
    'CreateOrUpdateInput',
    'CreateInputValidator',
    'UpdateInputValidator',
    // 'Localization',
  ];
  const modifies = ['AutoMapperProfile', 'Permissions'];
  // plop.setGenerator('Repository', {
  //   description: 'Generate a new Repository',
  //   prompts: [
  //     {
  //       type: 'input',
  //       name: 'name',
  //       message: 'What is the name of entity?',
  //       default: 'Order',
  //       validate: (value, prompts) => {
  //         console.log(`input name: ${value}`);
  //         if (!value) {
  //           return 'Please enter a name for your CRUD services.';
  //         }
  //         prompts.pluralName = plop.getHelper('pascalCase')(pluralize(value));
  //         return true;
  //       },
  //     },
  //     {
  //       type: 'list',
  //       name: 'primaryKey',
  //       message: 'What is the type of primary key?',
  //       default: 'Guid',
  //       choices: ['Guid', 'String', 'Long', 'Int'],
  //     },
  //   ],
  //   actions: (args) => {
  //     const actions = [];
  //     args = { ...args, namespace, project };
  //     console.info('args', args);
  //     // IRepository
  //     actions.push({
  //       type: 'add',
  //       path: `${srcPrefix}.Domain/{{pascalCase pluralName}}/I{{pascalCase name}}Repository.cs`,
  //       templateFile: 'plop-templates/IRepository.hbs',
  //     });
  //     // Repository

  //     actions.push({
  //       type: 'add',
  //       path: `${srcPrefix}.EntityFrameworkCore/{{pascalCase pluralName}}/{{pascalCase name}}Repository.cs`,
  //       templateFile: 'plop-templates/Repository.hbs',
  //     });

  //     // 跳过已存在的文件
  //     for (const item of actions.filter((x) => x.type == 'add')) {
  //       const filePath = path
  //         .join(__dirname, item.path)
  //         .replace('{{pascalCase pluralName}}', args.pluralName)
  //         .replace('{{pascalCase name}}', args.name);
  //       item.isExists = fs.existsSync(filePath);
  //       console.warn(item.isExists, `File ${filePath} `);
  //       item.isExists &&
  //         console.error(
  //           `Exists file: '${filePath}' already exists, skipping creation.`,
  //         );
  //     }

  //     return [
  //       'Start',
  //       ...actions
  //         .map((x) => ({ ...x, data: { ...x.data, ...args } }))
  //         .filter((x) => !x.isExists),
  //       'End',
  //     ];
  //   },
  // });
  // Define a generator for a new Net9 component
  plop.setGenerator('CRUD', {
    description: 'Generate a new CRUD services',
    prompts: [
      {
        type: 'input',
        name: 'name',
        message: 'What is the name of entity?',
        default: 'Device',
        validate: (value, prompts) => {
          console.log(`input name: ${value}`);
          if (!value) {
            return 'Please enter a name for your CRUD services.';
          }
          prompts.pluralName = plop.getHelper('pascalCase')(pluralize(value));
          console.warn(`pluralize name: ${prompts.pluralName}`);
          console.warn(`namespace: ${namespace}`);
          console.warn(`project: ${project}`);
          return true;
        },
      },
      {
        type: 'list',
        name: 'primaryKey',
        message: 'What is the type of primary key?',
        default: 'Guid',
        choices: ['Guid', 'String', 'Long', 'Int'],
      },
      ...[...adds, ...modifies].map((x, i) => ({
        type: 'confirm',
        name: `is${x}`,
        message: `${i < adds.length ? 'Generate' : 'Modify'} ${x}?`,
        default: true,
      })),
      {
        type: 'list',
        name: 'confirm',
        message: 'Show configuration?',
        default: 'Yes',
        choices: ['Yes', 'No'],
        validate: (value, prompts) => {
          console.log(`prompts: ${prompts}`);
          return true;
        },
      },
    ],
    actions: (args) => {
      console.info('args', args);
      args = { ...args, namespace, project };

      if (args.confirm !== 'Yes') {
        console.info('Cancelled.');
        return [];
      }

      // return [];
      const actions = [];
      // IRepository
      args.isRepository &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Domain/{{pascalCase pluralName}}/I{{pascalCase name}}Repository.cs`,
          templateFile: 'plop-templates/IRepository.hbs',
        });
      // Repository
      args.isRepository &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.EntityFrameworkCore/{{pascalCase pluralName}}/{{pascalCase name}}Repository.cs`,
          templateFile: 'plop-templates/Repository.hbs',
        });
      // Manager
      args.isManager &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Domain/{{pascalCase pluralName}}/{{pascalCase name}}Manager.cs`,
          templateFile: 'plop-templates/Manager.hbs',
        });
      // IManager
      args.isManager &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Domain/{{pascalCase pluralName}}/I{{pascalCase name}}Manager.cs`,
          templateFile: 'plop-templates/IManager.hbs',
        });
      // AppService
      args.isAppService &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application/{{pascalCase pluralName}}/{{pascalCase name}}AppService.cs`,
          templateFile: 'plop-templates/AppService.hbs',
        });
      // IAppService
      args.isAppService &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application.Contracts/{{pascalCase pluralName}}/I{{pascalCase name}}AppService.cs`,
          templateFile: 'plop-templates/IAppService.hbs',
        });
      // GetListInput
      args.isGetListInput &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application.Contracts/{{pascalCase pluralName}}/{{pascalCase name}}GetListInput.cs`,
          templateFile: 'plop-templates/Dtos/GetListInput.hbs',
        });
      // Dto
      args.isDto &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application.Contracts/{{pascalCase pluralName}}/{{pascalCase name}}Dto.cs`,
          templateFile: 'plop-templates/Dtos/Dto.hbs',
        });
      // CreateInput
      args.isCreateInput &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application.Contracts/{{pascalCase pluralName}}/{{pascalCase name}}CreateInput.cs`,
          templateFile: 'plop-templates/Dtos/CreateInput.hbs',
        });
      // UpdateInput
      args.isUpdateInput &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application.Contracts/{{pascalCase pluralName}}/{{pascalCase name}}UpdateInput.cs`,
          templateFile: 'plop-templates/Dtos/UpdateInput.hbs',
        });
      // SampleDto
      args.isSampleDto &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application.Contracts/{{pascalCase pluralName}}/{{pascalCase name}}SampleDto.cs`,
          templateFile: 'plop-templates/Dtos/SampleDto.hbs',
        });
      // DetailDto
      args.isDetailDto &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application.Contracts/{{pascalCase pluralName}}/{{pascalCase name}}DetailDto.cs`,
          templateFile: 'plop-templates/Dtos/DetailDto.hbs',
        });
      // CreateOrUpdateInput
      args.isCreateOrUpdateInput &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application.Contracts/{{pascalCase pluralName}}/{{pascalCase name}}CreateOrUpdateInput.cs`,
          templateFile: 'plop-templates/Dtos/CreateOrUpdateInput.hbs',
        });
      // CreateInputValidator
      args.isCreateInputValidator &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application/{{pascalCase pluralName}}/{{pascalCase name}}CreateInputValidator.cs`,
          templateFile: 'plop-templates/Validators/CreateInputValidator.hbs',
        });
      // UpdateInputValidator
      args.isUpdateInputValidator &&
        actions.push({
          type: 'add',
          path: `${srcPrefix}.Application/{{pascalCase pluralName}}/{{pascalCase name}}UpdateInputValidator.cs`,
          templateFile: 'plop-templates/Validators/UpdateInputValidator.hbs',
        });
      // AutoMapperProfile
      args.isAutoMapperProfile &&
        actions.push({
          type: 'modify',
          path: `${srcPrefix}.Application/AutoMappers/${project}ApplicationAutoMapperProfile.cs`,
          pattern:
            /\/\*---------code-generator-mapper: Do not modify or delete this line of comments--------\*\//g,
          template: `
        //{{pascalCase name}}
        CreateMap<{{pascalCase name}}, {{pascalCase name}}Dto>();
        CreateMap<{{pascalCase name}}, {{pascalCase name}}SampleDto>();
        CreateMap<{{pascalCase name}}, {{pascalCase name}}DetailDto>();
        CreateMap<{{pascalCase name}}CreateInput, {{pascalCase name}}>(MemberList.None).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<{{pascalCase name}}UpdateInput, {{pascalCase name}}>(MemberList.None).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        \r\n
        /*---------code-generator-mapper: Do not modify or delete this line of comments--------*/`,
        });
      // AutoMapperProfile
      args.isAutoMapperProfile &&
        actions.push({
          type: 'modify',
          path: `${srcPrefix}.Application/AutoMappers/${project}ApplicationAutoMapperProfile.cs`,
          pattern:
            /\/\*---------code-generator-namespace: Do not modify or delete this line of comments--------\*\//g,
          template: `using {{namespace}}.{{pascalCase pluralName}};
/*---------code-generator-namespace: Do not modify or delete this line of comments--------*/`,
        });
      // Permissions
      args.isPermissions &&
        actions.push({
          type: 'modify',
          path: `${srcPrefix}.Application.Contracts/Permissions/${project}Permissions.cs`,
          pattern:
            /\/\*---------code-generator-permissions: Do not modify or delete this line of comments--------\*\//g,
          template: `
    /// <summary>
    /// {{pascalCase name}}
    /// </summary>
    public class {{pascalCase name}}Permissions
    {
        public const string Default = GroupName + $".{nameof({{pascalCase name}}Permissions)}";
        public const string GetItem = Default + ".GetItem";
        public const string GetList = Default + ".GetList";
        public const string Update = Default + ".Update";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
        public const string SetIsEnabled = Default + ".SetIsEnabled";
    }
    \r\n
    /*---------code-generator-permissions: Do not modify or delete this line of comments--------*/`,
        });

      // 跳过已存在的文件
      for (const item of actions.filter((x) => x.type == 'add')) {
        const filePath = path
          .join(__dirname, item.path)
          .replace('{{pascalCase pluralName}}', args.pluralName)
          .replace('{{pascalCase name}}', args.name);
        item.isExists = fs.existsSync(filePath);
        // console.warn(item.isExists, `File ${filePath} `);
        item.isExists &&
          console.error(
            `Exists file: '${filePath}' already exists, skipping creation.`,
          );
      }

      // localization

      const applyLocalization = () => {
        const files = getJsonFilesInDirectorySync(localizationDir);

        console.log(`files: `, files);

        const keys = {
          GetItem: '详情',
          GetList: '列表',
          Update: '修改',
          Create: '新增',
          Delete: '删除',
          SetIsEnabled: '设置启用状态',
        };

        files.forEach((x) => {
          // console.log(`x: `, x);
          const isCn = ['zh-Hant.json', 'zh-Hans.json'].some((y) =>
            x.includes(y),
          );
          const getValue = (key) => (isCn ? keys[key] : key);
          const keyName = `${project}Permissions.{{pascalCase name}}Permissions`;
          const pattern = `"code-generator-localization": "Do not modify or delete this line of comments"`;
          actions.push({
            type: 'modify',
            path: `${localizationDir}${x}`,
            pattern: new RegExp(pattern, 'g'),
            template: `
        "${keyName}": "{{pascalCase name}}",
        "${keyName}.GetItem": "${getValue('GetItem')}",
        "${keyName}.GetList": "${getValue('Update')}",
        "${keyName}.Update": "${getValue('GetItem')}",
        "${keyName}.Create": "${getValue('Create')}",
        "${keyName}.Delete": "${getValue('Delete')}",
        "${keyName}.SetIsEnabled": "${getValue('SetIsEnabled')}",
        \r\n
        ${pattern}`,
          });
        });
      };

      args.isLocalization && applyLocalization();

      return actions
        .map((x) => ({ ...x, data: { ...x.data, ...args } }))
        .filter((x) => !x.isExists);
    },
    after: function (answers, config, plop) {
      // 在所有动作执行完毕后调用此函数
      console.log(
        `Component "${plop.getHelper('pascalCase')(
          answers.name,
        )}" created successfully!`,
      );
    },
  });
};
