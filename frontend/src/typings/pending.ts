type Pending<T> = T | undefined;

export type PendingSection<T> = {
  [K in keyof T]: Pending<T[K]>;
};

export default Pending;
