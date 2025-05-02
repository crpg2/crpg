export interface Result<TData> {
  data: TData | null
  errors: Error[] | null
}

export interface Error {
  code: string
  type: ErrorType
  title: string | null
  detail: string | null
  traceId: string | null
  stackTrace: string | null
}

export enum ErrorType {
  InternalError = 'InternalError',
  Forbidden = 'Forbidden',
  Conflict = 'Conflict',
  NotFound = 'NotFound',
  Validation = 'Validation',
}
