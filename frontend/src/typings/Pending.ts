type Pending<T> = T | undefined;

export interface PendingValue<T> {
  value: T;
  errors?: { message: string; link?: string; detailedMessage?: string }[];
  warnings?: { message: string; link?: string; detailedMessage?: string }[];
  previousValue?: T;
  pending?: boolean;
}

export type PendingSection<T> = {
  [K in keyof T]?: PendingValue<T[K]>;
};

export default Pending;
