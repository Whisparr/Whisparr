import useApiQuery from 'Helpers/Hooks/useApiQuery';
import LogFile from 'typings/LogFile';

export default function useLogFiles() {
  return useApiQuery<LogFile[]>({
    path: '/log/file',
  });
}

export function useUpdateLogFiles() {
  return useApiQuery<LogFile[]>({
    path: '/log/file/update',
  });
}
