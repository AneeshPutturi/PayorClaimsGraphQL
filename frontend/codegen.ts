import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
  overwrite: true,
  schema: 'http://localhost:5194/graphql',
  documents: 'src/app/graphql/operations/**/*.graphql',
  generates: {
    'src/app/graphql/generated.ts': {
      plugins: [
        'typescript',
        'typescript-operations',
        'typescript-apollo-angular',
      ],
      config: {
        addExplicitOverride: true,
        namedClient: 'default',
        serviceSuffix: 'GQL',
        sdkClass: true,
      },
    },
  },
};

export default config;
