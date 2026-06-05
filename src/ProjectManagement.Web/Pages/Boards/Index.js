$(function () {
    var l = abp.localization.getResource('ProjectManagement');

    var projectApi = abp.appPath + 'api/app/project';
    var statusApi = abp.appPath + 'api/app/status/status';
    var taskApi = abp.appPath + 'api/app/work-task/work-task';
    var permissions = window.projectManagementBoardPermissions || {};
    var canCreateProject = permissions.canCreateProject === true;
    var canCreateStatus = permissions.canCreateStatus === true;
    var canEditStatus = permissions.canEditStatus === true;
    var canDeleteStatus = permissions.canDeleteStatus === true;
    var canCreateTask = permissions.canCreateTask === true;

    var selectedProjectStorageKey = 'projectmanagement.board.selectedProjectId';

    var state = {
        projects: [],
        statuses: [],
        tasks: [],
        selectedProjectId:
            localStorage.getItem(selectedProjectStorageKey) || '',
    };

    var $loading = $('#BoardLoading');
    var $emptyProjects = $('#BoardEmptyProjects');
    var $emptyTasks = $('#BoardEmptyTasks');
    var $columns = $('#BoardColumns');
    var $projectMenu = $('#BoardProjectMenu');
    var $projectDescription = $('#BoardProjectDescription');
    var $projectTitle = $('#BoardProjectTitle');
    var $projectCount = $('#BoardProjectCount');
    var $taskCount = $('#BoardTaskCount');
    var $statusCount = $('#BoardStatusCount');

    var createProjectModal = new abp.ModalManager(
        abp.appPath + 'Projects/CreateModal',
    );
    var createTaskModal = new abp.ModalManager(
        abp.appPath + 'WorkTasks/CreateModal',
    );
    var createStatusModal = new abp.ModalManager(
        abp.appPath + 'Statuses/CreateModal',
    );
    var editStatusModal = new abp.ModalManager(
        abp.appPath + 'Statuses/EditModal',
    );
    var detailTaskModal = new abp.ModalManager(
        abp.appPath + 'Boards/DetailModal',
    );

    createProjectModal.onResult(function () {
        loadBoard();
    });

    createTaskModal.onResult(function () {
        loadBoard();
    });

    createStatusModal.onResult(function () {
        loadBoard();
    });

    editStatusModal.onResult(function () {
        loadBoard();
    });

    detailTaskModal.onResult(function () {
        loadBoard();
    });

    document.addEventListener('projectCreated', function (e) {
        var createdProject = e && e.detail ? e.detail : null;

        if (createdProject && createdProject.id) {
            state.selectedProjectId = createdProject.id;
            localStorage.setItem(
                selectedProjectStorageKey,
                state.selectedProjectId,
            );
        }

        loadBoard();
    });
    document.addEventListener('workTaskCreated', loadBoard);
    document.addEventListener('workTaskUpdated', loadBoard);
    document.addEventListener('statusCreated', loadBoard);
    document.addEventListener('statusUpdated', loadBoard);

    $('#AddBoardButton').on('click', function (e) {
        e.preventDefault();
        if (!canCreateProject || $(this).prop('disabled')) {
            return;
        }

        createProjectModal.open();
    });

    $('#AddTaskButton').on('click', function (e) {
        e.preventDefault();
        if (!canCreateTask || $(this).prop('disabled')) {
            return;
        }

        if (!state.projects.length) {
            abp.notify.warn(l('NoProjectsFound'));
            return;
        }

        createTaskModal.open({ projectId: state.selectedProjectId });
    });

    $('#AddColumnButton').on('click', function (e) {
        e.preventDefault();
        if (!canCreateStatus || $(this).prop('disabled')) {
            return;
        }

        createStatusModal.open();
    });

    loadBoard();

    function loadBoard() {
        setLoading(true);

        $.when(
            abp.ajax({
                type: 'GET',
                url: projectApi,
                data: { skipCount: 0, maxResultCount: 1000, sorting: 'name' },
            }),
            abp.ajax({
                type: 'GET',
                url: statusApi,
                data: { skipCount: 0, maxResultCount: 1000, sorting: 'title' },
            }),
                abp.ajax({
                    type: 'GET',
                    url: taskApi,
                    data: {
                        skipCount: 0,
                        maxResultCount: 1000,
                        sorting: 'CreationTime desc',
                    },
                }),
        )
            .done(function (projectsResponse, statusesResponse, tasksResponse) {
                state.projects = normalizeArrayResponse(projectsResponse);
                state.statuses = normalizeArrayResponse(statusesResponse);
                state.tasks = normalizeArrayResponse(tasksResponse);

                if (!state.selectedProjectId && state.projects.length) {
                    state.selectedProjectId = state.projects[0].id;
                    localStorage.setItem(
                        selectedProjectStorageKey,
                        state.selectedProjectId,
                    );
                }

                if (
                    state.selectedProjectId &&
                    !state.projects.some(function (x) {
                        return x.id === state.selectedProjectId;
                    })
                ) {
                    state.selectedProjectId = state.projects.length
                        ? state.projects[0].id
                        : '';
                    localStorage.setItem(
                        selectedProjectStorageKey,
                        state.selectedProjectId,
                    );
                }

                renderBoard();
                setLoading(false);
            })
            .fail(function (error) {
                var message =
                    error &&
                    error.responseJSON &&
                    error.responseJSON.error &&
                    error.responseJSON.error.message
                        ? error.responseJSON.error.message
                        : l('CouldNotLoadTasks');
                abp.notify.error(message);
                setLoading(false);
            });
    }

    function renderBoard() {
        renderProjectSwitcher();

        var selectedProject =
            state.projects.find(function (x) {
                return x.id === state.selectedProjectId;
            }) || null;
        var projectTasks = state.tasks.filter(function (task) {
            return (
                !state.selectedProjectId ||
                task.projectId === state.selectedProjectId
            );
        });

        $projectTitle.text(
            selectedProject ? selectedProject.name || l('Board') : l('Board'),
        );
        $projectDescription.text(
            selectedProject ? selectedProject.description || '' : '',
        );
        $projectCount.text(state.projects.length + ' ' + l('Project'));
        $taskCount.text(projectTasks.length + ' ' + l('Tasks'));
        $statusCount.text(state.statuses.length + ' ' + l('Status'));

        if (!state.projects.length) {
            $emptyProjects.removeClass('d-none');
            $emptyTasks.addClass('d-none');
            $columns.addClass('d-none').empty();
            return;
        }

        $emptyProjects.addClass('d-none');

        if (!state.statuses.length) {
            $columns
                .removeClass('d-none')
                .empty()
                .append(
                    $('<div/>', { class: 'board-empty col-12' }).append(
                        $('<div/>', {
                            class: 'fw-semibold mb-1',
                            text: l('NoTasksForProject'),
                        }),
                        $('<div/>', {
                            text: 'Create a column first to organize tasks by status.',
                        }),
                    ),
                );
            $emptyTasks.addClass('d-none');
            return;
        }

        if (!projectTasks.length) {
            $emptyTasks.removeClass('d-none');
        } else {
            $emptyTasks.addClass('d-none');
        }

        $columns.removeClass('d-none').empty();

        state.statuses.forEach(function (status) {
            var tasksInStatus = projectTasks.filter(function (task) {
                return task.statusId === status.id;
            });

            $columns.append(buildColumn(status, tasksInStatus));
        });
    }

    function renderProjectSwitcher() {
        $projectMenu.empty();

        state.projects.forEach(function (project) {
            var isActive = project.id === state.selectedProjectId;
            var $item = $('<button/>', {
                type: 'button',
                class: 'dropdown-item ' + (isActive ? 'active' : ''),
                'data-project-id': project.id,
            });

            $item.append(
                $('<div/>', {
                    class: 'project-name',
                    text: project.name || l('Board'),
                }),
                $('<div/>', {
                    class: 'project-description',
                    text: project.description || '',
                }),
            );

            $item.on('click', function () {
                state.selectedProjectId = project.id;
                localStorage.setItem(
                    selectedProjectStorageKey,
                    state.selectedProjectId,
                );
                renderBoard();
            });

            $projectMenu.append($item);
        });
    }

    function buildColumn(status, tasks) {
        var $column = $('<div/>', { class: 'board-column' });
        var canShowStatusActions = canEditStatus || canDeleteStatus;
        var $actions = $('<div/>', { class: 'board-column__actions' });
        var $header = $('<div/>', { class: 'board-column__header' });
        var $left = $('<div/>', { class: 'board-column__header-left' });
        var $dot = $('<span/>', { class: 'board-column__dot' }).css(
            'background-color',
            status.color || '#94a3b8',
        );
        var $count = $('<span/>', {
            class: 'board-column__count',
            text: tasks.length,
        });
        var $title = $('<span/>', { text: status.title || '-' });

        $left.append($dot, $title);
        $header.append($left, $count);

        if (canShowStatusActions) {
            if (canEditStatus) {
                $actions.append(
                    $('<button/>', {
                        type: 'button',
                        class: 'board-column__action board-column__action--edit',
                        title: l('EditColumn'),
                        'aria-label': l('EditColumn'),
                    }).append(
                        $('<i/>', { class: 'fa-solid fa-pen-to-square' }),
                    ),
                );
            }

            if (canDeleteStatus) {
                $actions.append(
                    $('<button/>', {
                        type: 'button',
                        class: 'board-column__action board-column__action--delete',
                        title: l('Delete'),
                        'aria-label': l('Delete'),
                    }).append($('<i/>', { class: 'fa-solid fa-trash-can' })),
                );
            }

            if ($actions.children().length) {
                $column.append($actions);
            }
        }

        $header.append($left, $count);

        var $taskList = $('<div/>', { class: 'board-task-list' });

        if (!tasks.length) {
            $taskList.append(
                $('<div/>', {
                    class: 'board-empty',
                    text: 'No tasks in this column.',
                }),
            );
        } else {
            tasks.forEach(function (task) {
                $taskList.append(buildTaskCard(task, status));
            });
        }

        if (canEditStatus) {
            $column.on('click', '.board-column__action--edit', function (e) {
                e.preventDefault();
                e.stopPropagation();
                editStatusModal.open({ id: status.id });
            });
        }

        if (canDeleteStatus) {
            $column.on('click', '.board-column__action--delete', function (e) {
                e.preventDefault();
                e.stopPropagation();

                abp.message.confirm(
                    l('StatusDeleteConfirmationMessage'),
                    l('Delete'),
                    function (confirmed) {
                        if (!confirmed) {
                            return;
                        }

                        abp.ajax({
                            type: 'DELETE',
                            url:
                                abp.appPath +
                                'api/app/status/' +
                                status.id +
                                '/status',
                        })
                            .done(function () {
                                abp.notify.success(l('SuccessfullyDeleted'));
                                loadBoard();
                            })
                            .fail(function (error) {
                                var message =
                                    error &&
                                    error.responseJSON &&
                                    error.responseJSON.error &&
                                    error.responseJSON.error.message
                                        ? error.responseJSON.error.message
                                        : l('CouldNotDeleteStatus');
                                abp.notify.error(message);
                            });
                    },
                );
            });
        }

        $column.append($header, $taskList);
        return $column;
    }

    function buildTaskCard(task, status) {
        var $card = $('<div/>', {
            class: 'board-task-card',
            role: 'button',
            tabindex: 0,
        });
        var project = state.projects.find(function (x) {
            return x.id === task.projectId;
        });

        $card.css('--task-accent', status.color || '#4f46e5');

        $card.append(
            $('<div/>', {
                class: 'board-task-card__title',
                text: task.title || '-',
            }),
            $('<div/>', { class: 'board-task-card__meta' }).append(
                $('<span/>').text(project && project.name ? project.name : ''),
                $('<span/>').text(
                    formatDateRange(task.startedDate, task.endedDate),
                ),
            ),
            $('<div/>', { class: 'board-task-card__footer' }).append(
                $('<span/>', {
                    class: 'board-task-card__badge',
                    text: task.priorityName || '-',
                }),
                $('<span/>', {
                    class: 'small text-muted',
                    text: task.assigneeName || '-',
                }),
            ),
        );

        $card.on('click', function () {
            detailTaskModal.open({ id: task.id });
        });

        $card.on('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                detailTaskModal.open({ id: task.id });
            }
        });

        return $card;
    }

    function formatDateRange(startedDate, endedDate) {
        var start = formatDate(startedDate);
        var end = formatDate(endedDate);
        return start + ' - ' + end;
    }

    function formatDate(value) {
        if (!value) {
            return '-';
        }

        var date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return value;
        }

        return date.toLocaleDateString();
    }

    function setLoading(isLoading) {
        $loading.toggleClass('d-none', !isLoading);
        $columns.toggleClass('d-none', isLoading);
        $emptyProjects.toggleClass('d-none', true);
        $emptyTasks.toggleClass('d-none', true);
    }

    function normalizeArrayResponse(response) {
        if (!response) {
            return [];
        }

        if (Array.isArray(response)) {
            return response;
        }

        if (Array.isArray(response.items)) {
            return response.items;
        }

        return [];
    }
});
