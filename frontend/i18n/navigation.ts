import { createNavigation } from "next-intl/navigation";
import { routing } from "./routing";

// Todo componente/página dentro de (guest)/[locale]/ que navega entre rotas do próprio app
// importa Link/useRouter/usePathname/redirect daqui, não de next/link ou next/navigation —
// mantém o prefixo de locale ao navegar (Story 2.1, Task 5).
export const { Link, redirect, usePathname, useRouter, getPathname } = createNavigation(routing);
