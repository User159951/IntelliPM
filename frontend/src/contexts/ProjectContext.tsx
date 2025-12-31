import React, { createContext, useContext } from 'react';
import type { ProjectRole } from '@/types';

interface ProjectContextType {
  projectId: number;
  userRole: ProjectRole | null | undefined;
}

const ProjectContext = createContext<ProjectContextType | undefined>(undefined);

interface ProjectProviderProps {
  children: React.ReactNode;
  projectId: number;
  userRole: ProjectRole | null | undefined;
}

export const ProjectProvider: React.FC<ProjectProviderProps> = ({
  children,
  projectId,
  userRole,
}) => {
  return (
    <ProjectContext.Provider value={{ projectId, userRole }}>
      {children}
    </ProjectContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useProject = () => {
  const context = useContext(ProjectContext);
  if (context === undefined) {
    throw new Error('useProject must be used within a ProjectProvider');
  }
  return context;
};

