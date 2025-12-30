import { describe, it, expect, beforeEach } from 'vitest';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';
import { projectsApi } from './projects';
import type { CreateProjectRequest, UpdateProjectRequest } from '@/types';

describe('projectsApi', () => {
  beforeEach(() => {
    server.resetHandlers();
  });

  describe('getAll', () => {
    it('fetches all projects', async () => {
      const response = await projectsApi.getAll();

      expect(response).toHaveProperty('items');
      expect(Array.isArray(response.items)).toBe(true);
      expect(response.items.length).toBeGreaterThan(0);
    });
  });

  describe('getById', () => {
    it('fetches a project by ID', async () => {
      const project = await projectsApi.getById(1);

      expect(project).toHaveProperty('id');
      expect(project).toHaveProperty('name');
      expect(project.id).toBe(1);
    });

    it('handles project not found', async () => {
      server.use(
        http.get('http://localhost:5001/api/v1/Projects/999', () => {
          return HttpResponse.json(
            { error: 'Project not found' },
            { status: 404 }
          );
        })
      );

      await expect(projectsApi.getById(999)).rejects.toThrow();
    });
  });

  describe('create', () => {
    it('creates a new project', async () => {
      const projectData: CreateProjectRequest = {
        name: 'New Project',
        description: 'A new test project',
        type: 'Scrum',
        sprintDurationDays: 14,
      };

      const project = await projectsApi.create(projectData);

      expect(project).toHaveProperty('id');
      expect(project).toHaveProperty('name');
      expect(project.name).toBe('New Project');
      expect(project.type).toBe('Scrum');
    });
  });

  describe('update', () => {
    it('updates an existing project', async () => {
      const updateData: UpdateProjectRequest = {
        name: 'Updated Project Name',
        description: 'Updated description',
      };

      const updatedProject = await projectsApi.update(1, updateData);

      expect(updatedProject).toHaveProperty('id');
      expect(updatedProject.name).toBe('Updated Project Name');
    });

    it('handles update failure for non-existent project', async () => {
      server.use(
        http.put('http://localhost:5001/api/v1/Projects/999', () => {
          return HttpResponse.json(
            { error: 'Project not found' },
            { status: 404 }
          );
        })
      );

      const updateData: UpdateProjectRequest = {
        name: 'Updated Name',
      };

      await expect(projectsApi.update(999, updateData)).rejects.toThrow();
    });
  });

  describe('archive', () => {
    it('archives a project', async () => {
      await expect(projectsApi.archive(1)).resolves.not.toThrow();
    });
  });

  describe('deletePermanent', () => {
    it('permanently deletes a project', async () => {
      await expect(projectsApi.deletePermanent(1)).resolves.not.toThrow();
    });
  });

  describe('getMembers', () => {
    it('fetches project members', async () => {
      const response = await projectsApi.getMembers(1);

      // getMembers returns ProjectMember[] directly
      expect(Array.isArray(response)).toBe(true);
    });
  });
});

