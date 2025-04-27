// global state
const [activePageLoading, togglePageLoading] = useToggle()

export const usePageLoading = () => {
  return {
    activePageLoading,
    togglePageLoading,
  }
}
