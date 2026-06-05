$(function () {
    var l = abp.localization.getResource('ProjectManagement');
    var listUrl = abp.appPath + 'api/app/work-task/work-task';
    var projectApi = abp.appPath + 'api/app/project';
    var statusApi = abp.appPath + 'api/app/status/status';
    var priorityApi = abp.appPath + 'api/app/priority';
    var teamMemberApi = abp.appPath + 'api/app/team-member/team-member-dto';
    var permissions = window.projectManagementWorkTaskPermissions || {};
    var canEditWorkTask = permissions.canEdit === true;
    var canDeleteWorkTask = permissions.canDelete === true;

    var $loading = $('#WorkTaskLoading');
    var $empty = $('#WorkTaskEmpty');
    var $error = $('#WorkTaskError');
    var $errorMessage = $('#WorkTaskErrorMessage');
    var $tableWrapper = $('#WorkTaskTableWrapper');

    var $search = $('#WorkTaskSearch');
    var $searchButton = $('#WorkTaskSearchButton');
    var $filterToggle = $('#WorkTaskFilterToggle');
    var $filters = $('#WorkTaskFilters');
    var $filterStatus = $('#WorkTaskFilterStatus');
    var $filterPriority = $('#WorkTaskFilterPriority');
    var $filterAssignee = $('#WorkTaskFilterAssignee');
    var $filterProject = $('#WorkTaskFilterProject');
    var createModal = new abp.ModalManager(
        abp.appPath + 'WorkTasks/CreateModal',
    );
    var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + 'WorkTasks/EditModal',
    });

    var dataTable = $('#WorkTaskTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: false,
            processing: true,
            order: [[1, 'asc']],
            ajax: function (data, callback) {
                setState('loading');

                abp.ajax({
                    type: 'GET',
                    url: listUrl,
                    data: {
                        skipCount: data.start,
                        maxResultCount: data.length,
                        sorting: getSortingExpression(data),
                        filter: ($search.val() || '').trim(),
                        projectId: $filterProject.val() || null,
                        statusId: $filterStatus.val() || null,
                        priorityId: $filterPriority.val() || null,
                        assigneeId: $filterAssignee.val() || null,
                    },
                })
                    .done(function (response) {
                        var items =
                            response && Array.isArray(response.items)
                                ? response.items
                                : [];
                        var totalCount =
                            response && typeof response.totalCount === 'number'
                                ? response.totalCount
                                : items.length;

                        callback({
                            draw: data.draw,
                            recordsTotal: totalCount,
                            recordsFiltered: totalCount,
                            data: items,
                        });

                        setState(items.length ? 'content' : 'empty');
                    })
                    .fail(function (error) {
                        var message =
                            error &&
                            error.responseJSON &&
                            error.responseJSON.error &&
                            error.responseJSON.error.message
                                ? error.responseJSON.error.message
                                : l('CouldNotLoadTasks');

                        $errorMessage.text(message);
                        setState('error');

                        callback({
                            draw: data.draw,
                            recordsTotal: 0,
                            recordsFiltered: 0,
                            data: [],
                        });
                    });
            },
            columnDefs: [
                {
                    title: l('Actions'),
                    data: null,
                    width: '120px',
                    orderable: false,
                    render: function (data, type, row) {
                        var editDisabled = canEditWorkTask ? '' : ' disabled';
                        var deleteDisabled = canDeleteWorkTask
                            ? ''
                            : ' disabled';
                        var editAria = canEditWorkTask ? 'false' : 'true';
                        var deleteAria = canDeleteWorkTask ? 'false' : 'true';

                        return (
                            '<div class="dropdown">' +
                            '<button class="worktasks-action-btn dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">' +
                            '<i class="fa fa-cog"></i>' +
                            '<span>' +
                            l('Actions') +
                            '</span>' +
                            '</button>' +
                            '<ul class="dropdown-menu dropdown-menu-end">' +
                            '<li>' +
                            '<button type="button" class="dropdown-item worktasks-edit-action' +
                            editDisabled +
                            '" data-id="' +
                            row.id +
                            '" aria-disabled="' +
                            editAria +
                            '">' +
                            '<i class="fa fa-pencil-alt me-2"></i>' +
                            l('Edit') +
                            '</button>' +
                            '</li>' +
                            '<li>' +
                            '<button type="button" class="dropdown-item worktasks-delete-action text-danger' +
                            deleteDisabled +
                            '" data-id="' +
                            row.id +
                            '" aria-disabled="' +
                            deleteAria +
                            '">' +
                            '<i class="fa fa-trash-alt me-2"></i>' +
                            l('Delete') +
                            '</button>' +
                            '</li>' +
                            '</ul>' +
                            '</div>'
                        );
                    },
                },
                {
                    title: l('Title'),
                    data: 'title',
                    defaultContent: '-',
                },
                {
                    title: l('Project'),
                    data: 'projectName',
                    defaultContent: '-',
                },
                {
                    title: l('Status'),
                    data: 'statusName',
                    render: function (data) {
                        return buildBadge(data, 'status')[0].outerHTML;
                    },
                    defaultContent: '-',
                },
                {
                    title: l('Priority'),
                    data: 'priorityName',
                    render: function (data) {
                        return buildBadge(data, 'priority')[0].outerHTML;
                    },
                    defaultContent: '-',
                },
                {
                    title: l('Assignee'),
                    data: 'assigneeName',
                    defaultContent: '-',
                },
                {
                    title: l('StartDate'),
                    data: 'startedDate',
                    render: function (data) {
                        return formatDate(data);
                    },
                    defaultContent: '-',
                },
                {
                    title: l('EndDate'),
                    data: 'endedDate',
                    render: function (data) {
                        return formatDate(data);
                    },
                    defaultContent: '-',
                },
            ],
        }),
    );

    loadFilterOptions();

    // Force reload data when page loads to avoid showing stale cached data
    // This ensures background job changes are reflected immediately
    dataTable.ajax.reload(null, false);

    $filterToggle.on('click', function () {
        var isOpen = !$filters.hasClass('d-none');
        $filters.toggleClass('d-none', isOpen);
        $(this)
            .find('i')
            .toggleClass('fa-chevron-up', !isOpen)
            .toggleClass('fa-chevron-down', isOpen);
    });

    $('#RetryWorkTasksButton').on('click', function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    $searchButton.on('click', function () {
        dataTable.ajax.reload();
    });

    $search.on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            dataTable.ajax.reload();
        }
    });

    $search.on('input search', function () {
        dataTable.ajax.reload();
    });

    $filterStatus.on('change', function () {
        dataTable.ajax.reload();
    });
    $filterPriority.on('change', function () {
        dataTable.ajax.reload();
    });
    $filterAssignee.on('change', function () {
        dataTable.ajax.reload();
    });
    $filterProject.on('change', function () {
        dataTable.ajax.reload();
    });

    $('#NewWorkTaskButton').on('click', function (e) {
        e.preventDefault();
        createModal.open();
    });

    document.addEventListener('workTaskCreated', function () {
        dataTable.ajax.reload();
    });

    document.addEventListener('workTaskUpdated', function () {
        dataTable.ajax.reload();
    });

    // Auto-refresh data every 30 seconds to catch backend changes (e.g., from background jobs)
    setInterval(function () {
        dataTable.ajax.reload(null, false);
    }, 600000); // Reload every 10 minutes
    $('#WorkTaskTable').on('click', '.worktasks-edit-action', function (e) {
        e.preventDefault();
        if (!canEditWorkTask || $(this).hasClass('disabled')) {
            return;
        }

        var id = $(this).data('id');
        if (!id) {
            return;
        }

        editModal.open({ id: id });
    });

    // Delete handler
    $('#WorkTaskTable').on('click', '.worktasks-delete-action', function (e) {
        e.preventDefault();
        if (!canDeleteWorkTask || $(this).hasClass('disabled')) {
            return;
        }

        var id = $(this).data('id');
        if (!id) {
            return;
        }

        abp.message.confirm(
            l('TaskDeleteConfirmationMessage'),
            l('Delete'),
            function (confirmed) {
                if (!confirmed) {
                    return;
                }

                abp.ajax({
                    url: abp.appPath + 'api/app/work-task/' + id + '/work-task',
                    type: 'DELETE',
                })
                    .done(function () {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        dataTable.ajax.reload(null, false);
                    })
                    .fail(function (error) {
                        var message =
                            error &&
                            error.responseJSON &&
                            error.responseJSON.error &&
                            error.responseJSON.error.message
                                ? error.responseJSON.error.message
                                : l('CouldNotDelete');
                        abp.notify.error(message);
                    });
            },
        );
    });

    function setState(state) {
        $loading.toggleClass('d-none', state !== 'loading');
        $empty.toggleClass('d-none', state !== 'empty');
        $error.toggleClass('d-none', state !== 'error');
        $tableWrapper.toggleClass('d-none', state !== 'content');
    }

    function loadFilterOptions() {
        $.when(
            loadLookupOptions(projectApi, 'name'),
            loadLookupOptions(statusApi, 'title'),
            loadLookupOptions(priorityApi, 'title'),
            loadLookupOptions(teamMemberApi, 'name'),
        ).done(
            function (
                projectsResponse,
                statusesResponse,
                prioritiesResponse,
                assigneesResponse,
            ) {
                fillSelect(
                    $filterProject,
                    normalizeItems(projectsResponse),
                    'name',
                );
                fillSelect(
                    $filterStatus,
                    normalizeItems(statusesResponse),
                    'title',
                );
                fillSelect(
                    $filterPriority,
                    normalizeItems(prioritiesResponse),
                    'title',
                );
                fillSelect(
                    $filterAssignee,
                    normalizeItems(assigneesResponse),
                    'name',
                );
            },
        );
    }

    function loadLookupOptions(url, sorting) {
        return abp.ajax({
            type: 'GET',
            url: url,
            data: {
                skipCount: 0,
                maxResultCount: 1000,
                sorting: sorting,
            },
        });
    }

    function normalizeItems(response) {
        return response && response.items ? response.items : [];
    }

    function fillSelect($select, items, textField) {
        $select.empty();
        $select.append($('<option/>', { value: '', text: l('All') }));

        items.forEach(function (item) {
            $select.append(
                $('<option/>', {
                    value: item.id,
                    text: item[textField] || item.id,
                }),
            );
        });
    }

    function getSortingExpression(data) {
        if (!data.order || !data.order.length) {
            return 'CreationTime desc';
        }

        var order = data.order[0];
        var column = data.columns[order.column] || {};
        var direction = order.dir === 'desc' ? 'desc' : 'asc';
        var columnName = column.data || '';

        if (columnName === 'title') {
            return 'Title ' + direction;
        }

        if (columnName === 'startedDate') {
            return 'StartedTime ' + direction;
        }

        if (columnName === 'endedDate') {
            return 'EndedTime ' + direction;
        }

        if (columnName === 'projectName') {
            return 'ProjectId ' + direction;
        }

        if (columnName === 'statusName') {
            return 'StatusId ' + direction;
        }

        if (columnName === 'priorityName') {
            return 'PriorityId ' + direction;
        }

        if (columnName === 'assigneeName') {
            return 'AssigneeId ' + direction;
        }

        return 'CreationTime ' + direction;
    }

    function buildBadge(value, type) {
        var text = value || '-';
        var key = (value || '').toLowerCase().replace(/\s+/g, '');
        var className = 'worktasks-badge ' + type + '-default';

        if (type === 'status') {
            if (key === 'todo') {
                className = 'worktasks-badge status-todo';
            } else if (key === 'inprogress' || key === 'in-progress') {
                className = 'worktasks-badge status-inprogress';
            } else if (key === 'review') {
                className = 'worktasks-badge status-review';
            } else if (key === 'done') {
                className = 'worktasks-badge status-done';
            }
        }

        if (type === 'priority') {
            if (key === 'low') {
                className = 'worktasks-badge priority-low';
            } else if (key === 'medium') {
                className = 'worktasks-badge priority-medium';
            } else if (key === 'high') {
                className = 'worktasks-badge priority-high';
            } else if (key === 'critical') {
                className = 'worktasks-badge priority-critical';
            }
        }

        return $('<span/>', { class: className }).text(text);
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
});
