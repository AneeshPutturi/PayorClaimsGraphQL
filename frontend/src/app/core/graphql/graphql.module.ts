import { NgModule } from '@angular/core';
import { APOLLO_OPTIONS } from 'apollo-angular';
import { HttpLink } from 'apollo-angular/http';
import { InMemoryCache, split, ApolloLink } from '@apollo/client/core';
import type { ApolloClient } from '@apollo/client';
import { ErrorLink } from '@apollo/client/link/error';
import { CombinedGraphQLErrors } from '@apollo/client/errors';
import { setContext } from '@apollo/client/link/context';
import { getMainDefinition } from '@apollo/client/utilities';
import { GraphQLWsLink } from '@apollo/client/link/subscriptions';
import { createClient } from 'graphql-ws';
import { HttpHeaders } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';

export function createApollo(
  httpLink: HttpLink,
  authService: AuthService,
  snackBar: MatSnackBar,
  router: Router
): ApolloClient.Options {
  const http = httpLink.create({ uri: environment.apiUrl });

  const authLink = setContext(() => {
    const token = authService.token;
    if (!token) return {};
    return {
      headers: new HttpHeaders().set('Authorization', `Bearer ${token}`),
    };
  });

  const errorLink = new ErrorLink(({ error }) => {
    if (CombinedGraphQLErrors.is(error)) {
      for (const err of error.errors) {
        const code = (err.extensions as Record<string, unknown>)?.['code'] ?? '';
        switch (code) {
          case 'CONCURRENCY_CONFLICT':
            snackBar.open('Data was modified by another user. Please reload.', 'Reload', { duration: 8000 });
            break;
          case 'VALIDATION_FAILED':
            snackBar.open('Validation error: ' + err.message, 'OK', { duration: 6000 });
            break;
          case 'QUERY_TOO_DEEP':
          case 'QUERY_TOO_COMPLEX':
            snackBar.open('Query too complex. Simplify your request.', 'OK', { duration: 5000 });
            break;
          case 'PERSISTED_ONLY':
          case 'UNKNOWN_HASH':
          case 'MISSING_HASH':
            snackBar.open('Persisted queries are enabled. Raw queries not allowed.', 'OK', { duration: 5000 });
            break;
          case 'FORBIDDEN':
            router.navigate(['/forbidden']);
            break;
          case 'UNAUTHORIZED':
            authService.logout();
            router.navigate(['/login']);
            break;
          default:
            snackBar.open(err.message, 'OK', { duration: 5000 });
        }
      }
    } else {
      // Network / other error
      const anyError = error as unknown as Record<string, unknown>;
      const status = anyError['status'] ?? anyError['statusCode'];
      if (status === 401) {
        authService.logout();
        router.navigate(['/login']);
      } else if (status === 403) {
        router.navigate(['/forbidden']);
      } else {
        snackBar.open('Network error. Check your connection.', 'OK', { duration: 5000 });
      }
    }
  });

  const wsLink = new GraphQLWsLink(
    createClient({
      url: environment.wsUrl,
      connectionParams: () => {
        const token = authService.token;
        return token ? { Authorization: `Bearer ${token}` } : {};
      },
    })
  );

  const splitLink = split(
    ({ query }) => {
      const def = getMainDefinition(query);
      return def.kind === 'OperationDefinition' && def.operation === 'subscription';
    },
    wsLink,
    ApolloLink.from([errorLink, authLink, http])
  );

  return {
    link: splitLink,
    cache: new InMemoryCache(),
    defaultOptions: {
      watchQuery: { errorPolicy: 'all' },
      query: { errorPolicy: 'all' },
      mutate: { errorPolicy: 'all' },
    },
  };
}

@NgModule({
  providers: [
    {
      provide: APOLLO_OPTIONS,
      useFactory: createApollo,
      deps: [HttpLink, AuthService, MatSnackBar, Router],
    },
  ],
})
export class GraphqlModule {}
